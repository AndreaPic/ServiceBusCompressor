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
    internal class ReaderExtender<TClient> : MessageReader
       where TClient : IReceiverClient 
    {

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
        }
        /// <summary>
        /// Initialize new instance for the client with explicit storage settings for large messages
        /// </summary>
        /// <param name="client">Queue or topic to extend</param>
        /// <param name="settingData">Storage settings for large messages</param>
        internal ReaderExtender(TClient client, StorageSettingData settingData) : base (settingData)
        {
            Client = client;
        }


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
        /// Subscribe an action for reading message from queue or topic
        /// </summary>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        public void Subscribe(Action<MessageReceivedEventArgs> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
            RegisterForMessage();
        }

        /// <summary>
        /// Handle the received message
        /// </summary>
        /// <param name="receivedMessage">message received from service bus</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        protected override async Task MessageReceivedHandler(Message receivedMessage, CancellationToken token)
        {
            try
            {
                await base.MessageReceivedHandler(receivedMessage, token);
                await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
            }
            catch
            {
                await Client.AbandonAsync(receivedMessage.SystemProperties.LockToken);
                throw;
            }
        }

    }
}
