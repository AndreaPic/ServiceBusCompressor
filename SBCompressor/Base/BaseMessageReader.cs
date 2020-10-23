using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SBCompressor.Configuration;
using SBCompressor.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// base class for queue or topic reader\subscriptor
    /// </summary>
    /// <typeparam name="TClient">
    /// Concrete type of client\reader\subscriptor
    /// </typeparam>
    public abstract class BaseMessageReader<TClient> : ConnectorCore, IMessageReader
       where TClient : ClientEntity, IReceiverClient
    {
        /// <summary>
        /// Initialize for queue of the connection string
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">name of the connection string in the setting file</param>
        public BaseMessageReader(string entityName, string connectionStringName) : base (entityName, connectionStringName)
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
        }
        public BaseMessageReader(string entityName, string connectionStringName,
            StorageSettingData settingData) : this(entityName, connectionStringName)
        {
            CurrentSettingData = settingData;
        }

        private StorageSettingData CurrentSettingData { get; set; }


        /// <summary>
        /// Subscribe to the queue or topic of the service bus
        /// </summary>
        /// <returns></returns>
        public async Task EnsureSubscribe(Action<MessageReceivedEventArgs> onMessageReceived)
        {
            NamespaceInfo namespaceInfo = await CreateNamespaceInfoAsync();
            bool existSubscription = await CheckExistSubscription();
            if (existSubscription)
            {
                CurrentSubscriptionClient = CreateSubscriptionClient();
                OnMessageReceived = onMessageReceived;
                RegisterForMessage();
            }
        }

        /// <summary>
        /// Current client
        /// </summary>
        protected TClient CurrentSubscriptionClient { get; private set; }

        /// <summary>
        /// Storage manager for very large message
        /// </summary>
        private MessageStorage storage;
        /// <summary>
        /// Storage manager for very large message
        /// </summary>
        private MessageStorage Storage 
        { 
            get
            {
                if (storage==null)
                {
                    if (CurrentSettingData != null)
                    {
                        storage = new MessageStorage(CurrentSettingData);
                    }
                    else
                    {
                        storage = new MessageStorage();
                    }
                }
                return storage;
            } 
        }
        /// <summary>
        /// Container for chunks of large messages
        /// </summary>
        private ConcurrentDictionary<string, List<byte[]>> ChunkDictionary { get; set; }

        /// <summary>
        /// Close the client
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            await CurrentSubscriptionClient.CloseAsync();
        }

        /// <summary>
        /// Register client for messages
        /// </summary>
        protected virtual void RegisterForMessage()
        {
            MessageHandlerOptions options = new MessageHandlerOptions(ExceptionReceivedHandler);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = 1;
            CurrentSubscriptionClient.RegisterMessageHandler(MessageReceivedHandler, options);
        }
        /// <summary>
        /// Handler in caso of exceptions reading message
        /// </summary>
        /// <param name="exceptionReceivedEventArgs">information about the exceptions</param>
        /// <returns></returns>
        virtual protected Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Create the right instance of the client
        /// </summary>
        /// <returns>Client for the queue or topic</returns>
        abstract protected TClient CreateSubscriptionClient();


        /// <summary>
        /// Handle the received message
        /// </summary>
        /// <param name="receivedMessage">message received from service bus</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        public async Task MessageReceivedHandler(Message receivedMessage, CancellationToken token)
        {
            try
            {
                // Process message from subscription.
                if (receivedMessage.UserProperties.ContainsKey(MessageFactory.MessageModePropertyName))
                {
                    var prop = receivedMessage.UserProperties[MessageFactory.MessageModePropertyName];
                    string propName = Convert.ToString(prop);
                    if (!string.IsNullOrEmpty(propName))
                    {
                        MessageModes messageMode;
                        bool parsed = Enum.TryParse(propName, out messageMode);
                        if (parsed)
                        {
                            EventMessage msg = null;
                            switch (messageMode)
                            {
                                case MessageModes.Simple:
                                    msg = ReaderHandler.GetSimpleMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                case MessageModes.GZip:
                                    msg = await ReaderHandler.GetZippedMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await ReaderHandler.GetChunkedMessage(receivedMessage, ChunkDictionary);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    if (msg != null)
                                    {
                                        MessageReceived(msg, receivedMessage);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await ReaderHandler.GetStoredMessage(receivedMessage, Storage);
                                    MessageReceived(msg, receivedMessage);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                await CurrentSubscriptionClient.AbandonAsync(receivedMessage.SystemProperties.LockToken);
                throw;
            }
        }


        /*
        /// <summary>
        /// Event raised when message is ready to be readed 
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;
        */

        private Action<MessageReceivedEventArgs> OnMessageReceived;

        /// <summary>
        /// Raise OnMessageReceived event
        /// </summary>
        /// <param name="message">Message managed by this library</param>
        /// <param name="receivedMessage">Original message from queue or topic</param>
        protected virtual void MessageReceived(EventMessage message, Message receivedMessage)
        {
            OnMessageReceived?.Invoke(new MessageReceivedEventArgs(message, receivedMessage));
        }

    }
}
