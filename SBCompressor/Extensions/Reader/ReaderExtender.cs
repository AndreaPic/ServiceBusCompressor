using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using SBCompressor.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Reader
{
    /// <summary>
    /// Utility for topic or queue reader extension
    /// </summary>
    /// <typeparam name="TClient">ReceiverClient for the messages</typeparam>
    internal class ReaderExtender<TClient>
       where TClient : IReceiverClient 
    {
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
                if (storage == null)
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
        /// Current client of queue or topic
        /// </summary>
        private TClient Client { get; set; }

        /// <summary>
        /// Get ghe current client of queue or topic
        /// </summary>
        /// <returns></returns>
        virtual protected TClient GetClient()
        {
            return Client;
        }

        /// <summary>
        /// Initialize new instance for the client argument
        /// </summary>
        /// <param name="client">Queue or topic to extend</param>
        internal ReaderExtender(TClient client)
        {
            Client = client;
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
        }
        internal ReaderExtender(TClient client, StorageSettingData settingData) : this (client)
        {
            CurrentSettingData = settingData;
        }

        private StorageSettingData CurrentSettingData { get; set; }

        /// <summary>
        /// Register client for messages
        /// </summary>
        protected virtual void RegisterForMessage()
        {
            MessageHandlerOptions options = new MessageHandlerOptions(ExceptionReceivedHandler);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = 1;
            Client.RegisterMessageHandler(MessageReceivedHandler, options);
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
        /// Handle the received message
        /// </summary>
        /// <param name="receivedMessage">message received from service bus</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        private async Task MessageReceivedHandler(Message receivedMessage, CancellationToken token)
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
                                    await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                case MessageModes.GZip:
                                    msg = await ReaderHandler.GetZippedMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await ReaderHandler.GetChunkedMessage(receivedMessage,ChunkDictionary);
                                    await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    if (msg != null)
                                    {
                                        MessageReceived(msg, receivedMessage);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await ReaderHandler.GetStoredMessage(receivedMessage,Storage);
                                    MessageReceived(msg, receivedMessage);
                                    await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
            }
        }

        /// <summary>
        /// Subscribe an action for reading message from queue or topic
        /// </summary>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        public void Subscribe(Action<MessageReceivedEventArgs> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
            RegisterForMessage();
        }

        /// <summary>
        /// Action to invoke when a message arrive
        /// </summary>
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
