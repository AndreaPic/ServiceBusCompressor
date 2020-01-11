using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure;
using Microsoft.Azure.ServiceBus.Management;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Azure.ServiceBus.Core;
using System.Collections.Generic;
using System.Net.Http;

namespace SBCompressor
{
    /// <summary>
    /// Base class for queue or topic sender to service bus
    /// </summary>
    /// <typeparam name="TClient">Type of the client</typeparam>
    public abstract class BaseConnector<TClient> : ConnectorCore
        where TClient : ClientEntity, ISenderClient
    {
        /// <summary>
        /// Initialize this type
        /// </summary>
        static BaseConnector()
        {
            Clients = new System.Collections.Concurrent.ConcurrentDictionary<string, TClient>();
        }
        /// <summary>
        /// Create a message sender to service bus
        /// </summary>
        /// <param name="entityName">Queue Name</param>
        /// <param name="connectionStringName">Connection string name to use to look for the connection string in the settings file</param>
        public BaseConnector(string entityName, string connectionStringName) : base (entityName, connectionStringName)
        {
            Clients = new System.Collections.Concurrent.ConcurrentDictionary<string, TClient>();
        }
        private static System.Collections.Concurrent.ConcurrentDictionary<string, TClient> Clients { get; set; }


        /// <summary>
        /// Add client to initialized client cache
        /// </summary>
        /// <param name="client">client to add to the cache</param>
        /// <returns>true if added</returns>
        private bool AddClientToInitialized(TClient client)
        {
            var ret = Clients.AddOrUpdate(EntityName, client, (e, q) =>
            {
                return client;
            });
            return ret != null;
        }
        /// <summary>
        /// Remove client from initialized client cache 
        /// </summary>
        /// <param name="client">client to remove from cache</param>
        /// <returns>true if removed</returns>
        private bool RemoveClientFromInitialized(out TClient client)
        {
            return Clients.TryRemove(EntityName, out client);
        }
        /// <summary>
        /// Get a client for the service bus
        /// </summary>
        /// <returns>
        /// Client configured for service bus
        /// </returns>
        protected async Task<TClient> GetClient()
        {
            TClient client = null;
            client = await EnsureInitialize();
            return client;
        }
        /// <summary>
        /// Verify if the client is initialized
        /// </summary>
        /// <returns>True if initialized</returns>
        private bool IsClientInitialized()
        {
            return Clients.ContainsKey(EntityName);
        }

        /// <summary>
        /// Ensure the client creation and initialization
        /// </summary>
        /// <returns>
        /// Client for cofnigured for service bus
        /// </returns>
        protected async Task<TClient> EnsureInitialize()
        {
            if (IsClientInitialized())
            {
                return Clients[EntityName];
            }

            var namespaceInfo = await CreateNamespaceInfoAsync();

            bool exist = CheckExistConnector(namespaceInfo);

            if (!exist)
            {
                await CreateInsideNamespaceAsync(namespaceInfo);
            }
            else
            {
                await ConfigureInsideNamespaceAsync(namespaceInfo);
            }

            TClient client = await CreateClientAsync(namespaceInfo);

            AddClientToInitialized(client);
            return client;
        }

        /// <summary>
        /// Verify if queue or topic exists
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo for the queue or topic</param>
        /// <returns>
        /// True if exists
        /// </returns>
        virtual protected bool CheckExistConnector(NamespaceInfo namespaceInfo)
        {
            return (namespaceInfo != null); 
        }

        /// <summary>
        /// Create the right instance of the client for service bus
        /// </summary>
        /// <param name="namespaceInfo">
        /// NamespaceInfo of the service bus
        /// </param>
        /// <returns>
        /// Client of serivce bus
        /// </returns>
        abstract protected Task<TClient> CreateClientAsync(NamespaceInfo namespaceInfo);
        /// <summary>
        /// Create queue or topic for inside service bus
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo of the service bus</param>
        /// <returns></returns>
        protected abstract Task CreateInsideNamespaceAsync(NamespaceInfo namespaceInfo);
        /// <summary>
        /// Configure servicebus inside namespace
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo of the service bus</param>
        /// <returns></returns>
        protected virtual Task ConfigureInsideNamespaceAsync(NamespaceInfo namespaceInfo) { return Task.CompletedTask; }

        /// <summary>
        /// Close the client
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            TClient client;
            bool removed = RemoveClientFromInitialized(out client);
            if (removed)
            {
                await client.CloseAsync();
            }
        }
        /// <summary>
        /// Close all clients
        /// </summary>
        /// <returns></returns>
        internal static async Task<bool> CloseAsync(string entityName)
        {
            TClient client;
            bool removed = Clients.TryRemove(entityName, out client);
            if (removed)
            {
                await client.CloseAsync();
            }
            return removed;
        }

        /// <summary>
        /// Create sender for the queue or topic
        /// </summary>
        /// <returns>sender configured for the queue or topic</returns>
        private async Task<ISenderClient> CreateMessageSender()
        {
            return await EnsureInitialize(); 
        }

        /// <summary>
        /// Send message to service bus using standard client
        /// </summary>
        /// <param name="message">message for queue or topic</param>
        /// <returns></returns>
        protected async Task ToSendAsync(Message message)
        {
            var sender = await CreateMessageSender();
            await sender.SendAsync(message);
        }

        /// <summary>
        /// Send messagea to service bus using standard client
        /// </summary>
        /// <param name="messages">list of messages for queue or topic</param>
        /// <returns></returns>
        protected async Task ToSendBatchAsync(List<Message> messages)
        {
            var sender = await CreateMessageSender();
            await sender.SendAsync(messages);
        }

        /// <summary>
        /// path for settings file to look for strategy
        /// </summary>
        private const string VeryLargeStrategiyettingConfig = "VeryLargeMessage:Strategy";
        /// <summary>
        /// Strategy used for very large message
        /// </summary>
        private VeryLargeMessageStrategy? strategy;

        /// <summary>
        /// Get the strategy to use for very large message
        /// </summary>
        /// <returns>Current strategy to use for very large message</returns>
        private VeryLargeMessageStrategy GetVeryLargeMessageStrategy()
        {
            if (!strategy.HasValue)
            {
                strategy = VeryLargeMessageStrategy.Storage;
                string strategyConfig = Settings.CurrentConfiguration[VeryLargeStrategiyettingConfig];
                if (!string.IsNullOrEmpty(strategyConfig))
                {
                    VeryLargeMessageStrategy tmp;
                    bool parsed = Enum.TryParse(strategyConfig, out tmp);
                    if (parsed)
                    {
                        strategy = tmp;
                    }
                }
            }
            return strategy.Value;
        }

        /// <summary>
        /// Send EventMessage to service bus
        /// </summary>
        /// <param name="message">string message to send. It could be a json.</param>
        /// <returns></returns>
        public async Task SendAsync(string message)
        {
            EventMessage eventMessage = new EventMessage();
            eventMessage.Body = message;
            await SendAsync(eventMessage);
        }

        /// <summary>
        /// Send EventMessage to service bus
        /// </summary>
        /// <param name="eventMessage">message to send</param>
        /// <returns></returns>
        public async Task SendAsync(EventMessage eventMessage)
        {
            Message brokeredMessage = null;
            MessageFactory messageFactory = new MessageFactory();
            var strategy = GetVeryLargeMessageStrategy();
            var messageWrapper = messageFactory.CreateMessageFromEvent(eventMessage, strategy);
            switch (messageWrapper.MessageMode)
            {
                case MessageModes.Simple:
                    brokeredMessage = messageWrapper.Message;
                    await ToSendAsync(brokeredMessage);
                    break;
                case MessageModes.GZip:
                    brokeredMessage = messageWrapper.Message;
                    await ToSendAsync(brokeredMessage);
                    break;
                case MessageModes.Chunk:
                    await ToSendBatchAsync(messageWrapper.Messages);
                    break;
                case MessageModes.Storage:
                    brokeredMessage = messageWrapper.Message;
                    await ToSendAsync(brokeredMessage);
                    break;
            }
        }
    }
}
