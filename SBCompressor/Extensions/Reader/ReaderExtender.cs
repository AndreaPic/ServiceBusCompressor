#if NET6_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
#endif
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
#if NETCOREAPP3_1 || NET5_0
       where TClient : IReceiverClient 
#endif
#if NET6_0
       where TClient : ServiceBusProcessor//ServiceBusReceiver
#endif
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
        /// Initialize new instance for the client with explicit storage settings for large messages
        /// </summary>
        /// <param name="client">Queue or topic to extend</param>
        /// <param name="settingData">Storage settings for large messages</param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        internal ReaderExtender(TClient client, StorageSettingData settingData, Type typeToDeserialize) : base(settingData, typeToDeserialize)
        {
            Client = client;
        }

        /// <summary>
        /// Initialize new instance for the client with explicit storage settings for large messages
        /// </summary>
        /// <param name="client">Queue or topic to extend</param>
        /// <param name="settingData">Storage settings for large messages</param>
        /// <param name="messageDeserializer">Object used to deserialize message from json</param>
        internal ReaderExtender(TClient client, StorageSettingData settingData, IMessageDeserializer messageDeserializer) : base(settingData, messageDeserializer)
        {
            Client = client;
        }

        /// <summary>
        /// Initialize new instance for the client with explicit storage settings for large messages
        /// </summary>
        /// <param name="client">Queue or topic to extend</param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        internal ReaderExtender(TClient client, Type typeToDeserialize) : base(typeToDeserialize)
        {
            Client = client;
        }

        /// <summary>
        /// Initialize new instance for the client with explicit storage settings for large messages
        /// </summary>
        /// <param name="client">Queue or topic to extend</param>
        /// <param name="messageDeserializer">Object used to deserialize message from json</param>
        internal ReaderExtender(TClient client, IMessageDeserializer messageDeserializer) : base(messageDeserializer)
        {
            Client = client;
        }

        /// <summary>
        /// Register client for messages
        /// </summary>
        protected virtual void RegisterForMessage()
        {
#if NET6_0
            Client.ProcessMessageAsync += MessageReceivedHandler;
            Client.ProcessErrorAsync += ErrorHandler;
#endif

#if NETCOREAPP3_1 || NET5_0
            MessageHandlerOptions options = new MessageHandlerOptions(ExceptionReceivedHandler);
            options.AutoComplete = false;
            options.MaxConcurrentCalls = 1;
            Client.RegisterMessageHandler(MessageReceivedHandler, options);
#endif
        }

        /// <summary>
        /// Subscribe an action for reading message from queue or topic
        /// </summary>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        public void Subscribe(Action<MessageReceivedEventArgs> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
//#if NETCOREAPP3_1 || NET5_0
            RegisterForMessage();
//#endif
        }

        /// <summary>
        /// Handle the received message
        /// </summary>
        /// <param name="receivedMessage">message received from service bus</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        protected override async Task MessageReceivedHandler(Message receivedMessage, CancellationToken token)
        {
            try
            {
                await base.MessageReceivedHandler(receivedMessage, token);
#if NETCOREAPP3_1
                await Client.CompleteAsync(receivedMessage.SystemProperties.LockToken);
#endif
            }
            catch
            {
                await Client.AbandonAsync(receivedMessage.SystemProperties.LockToken);
                throw;
            }
        }
#endif
#if NET6_0

        //protected override async Task MessageReceivedHandler(ServiceBusReceivedMessage receivedMessage, CancellationToken token)
        protected override async Task MessageReceivedHandler(ProcessMessageEventArgs receivedMessage)//, CancellationToken token)
        {
            try
            {
                await base.MessageReceivedHandler(receivedMessage);//, token);
                //await Client.CompleteMessageAsync(receivedMessage.Message);
                await receivedMessage.CompleteMessageAsync(receivedMessage.Message);
            }
            catch
            {
                //await Client.AbandonMessageAsync(receivedMessage);
                await receivedMessage.AbandonMessageAsync(receivedMessage.Message);
                throw;
            }
        }
        Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }
#endif
    }
}
