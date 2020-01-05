using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
    public abstract class BaseMessageReader<TClient> : ConnectorCore
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
                    storage = new MessageStorage();
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
                                    msg = GetSimpleMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                case MessageModes.GZip:
                                    msg = await GetZippedMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await GetChunkedMessage(receivedMessage);
                                    await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
                                    if (msg != null)
                                    {
                                        MessageReceived(msg, receivedMessage);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await GetStoredMessage(receivedMessage);
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
            catch (Exception)
            {
                await CurrentSubscriptionClient.CompleteAsync(receivedMessage.SystemProperties.LockToken);
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

        /// <summary>
        /// Get message from the storage
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <returns>Event message</returns>
        private async Task<EventMessage> GetStoredMessage(Message receivedMessage)
        {
            var bytes = Storage.DownloadMessage(receivedMessage.MessageId);
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            return message;
        }
        /// <summary>
        /// Get the message by chunks
        /// </summary>
        /// <param name="receivedMessage">Message from queue or topic</param>
        /// <returns>If chunks are completed return the recomposed completed message</returns>
        private async Task<EventMessage> GetChunkedMessage(Message receivedMessage)
        {
            object chunkGroupIdObject = receivedMessage.UserProperties[MessageFactory.MessageChunkGroupIdPropertyName];
            int? chunkIndex = receivedMessage.UserProperties[MessageFactory.MessageChunkIndex] as int?;
            string chunkGroupIdString = Convert.ToString(chunkGroupIdObject);

            byte[] bytes = receivedMessage.Body;
            List<byte[]> newBytes = new List<byte[]>();
            newBytes.Add(bytes);

            ChunkDictionary.AddOrUpdate(chunkGroupIdString, newBytes, (key, currentList) =>
            {
                currentList.Insert(chunkIndex.Value, bytes);
                return currentList;
            }
            );

            var chunkState = receivedMessage.UserProperties[MessageFactory.MessageChunkStatePropertyName];
            string chunkStateString = Convert.ToString(chunkState);
            ChunkStates currentChunkState;
            bool parsedState = Enum.TryParse(chunkStateString, out currentChunkState);

            EventMessage ret = null;
            if (parsedState && currentChunkState == ChunkStates.End)
            {
                byte[] completeMessage = new byte[0];
                List<byte[]> chunks = ChunkDictionary[chunkGroupIdString];
                foreach (var chunk in chunks)
                {
                    var tmp = completeMessage.Concat(chunk);
                    completeMessage = tmp.ToArray();
                }

                var jsonMessageString = await completeMessage.Unzip() as string;
                var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
                ret = message;
            }
            return ret;
        }
        /// <summary>
        /// Get the message from compressed state
        /// </summary>
        /// <param name="receivedMessage">Message from queue or topic</param>
        /// <returns>Uncompressed message</returns>
        private async Task<EventMessage> GetZippedMessage(Message receivedMessage)
        {
            var bytes = receivedMessage.Body;
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            return message;
        }
        /// <summary>
        /// Get message without extra working
        /// </summary>
        /// <param name="receivedMessage">Message from service bus</param>
        /// <returns>Message from service bus</returns>
        private EventMessage GetSimpleMessage(Message receivedMessage)
        {
            var jsonMessageString = Encoding.UTF8.GetString(receivedMessage.Body);
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            return message;
        }
    }
}
