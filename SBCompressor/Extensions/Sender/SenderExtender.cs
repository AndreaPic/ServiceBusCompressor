#if NET6_0 || NET7_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
#endif
using Newtonsoft.Json;
using SBCompressor.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Sender
{
    /// <summary>
    /// Utility class for extender a message sender to queue or topic
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    internal class SenderExtender<TClient>
#if NETCOREAPP3_1 || NET5_0
        where TClient : IClientEntity, ISenderClient
#endif
#if NET6_0 || NET7_0
       where TClient : ServiceBusSender
#endif

    {
        /// <summary>
        /// Current client of queue or topic
        /// </summary>
        private TClient Client { get; set; }

        private IMessageSerializer MessageSerializer { get; set; }

        /// <summary>
        /// Get the current client of queue or topic
        /// </summary>
        /// <returns>Current client of queue or topic</returns>
        virtual protected TClient GetClient()
        {
            return Client;
        }

        /// <summary>
        /// Create a new instance for the client
        /// </summary>
        /// <param name="client">Client to extend</param>
        internal SenderExtender(TClient client)
        {
            Client = client;
        }

        internal SenderExtender(TClient client, StorageSettingData settingData) : this(client)
        {
            CurrentSettingData = settingData;
        }

        /// <summary>
        /// Create a new instance for the client
        /// </summary>
        /// <param name="client">Client to extend</param>
        internal SenderExtender(TClient client, IMessageSerializer messageSerializer)
        {
            Client = client;
            MessageSerializer = messageSerializer;
        }

        internal SenderExtender(TClient client, StorageSettingData settingData, IMessageSerializer messageSerializer) : this(client)
        {
            MessageSerializer = messageSerializer;
            CurrentSettingData = settingData;
        }

        private StorageSettingData CurrentSettingData { get; set; }

        /// <summary>
        /// Send EventMessage to service bus
        /// </summary>
        /// <param name="message">string message to send. It could be a json.</param>
        /// <returns></returns>
        internal async Task SendAsync(string message)
        {
            EventMessage eventMessage = new EventMessage();
            eventMessage.Body = message;
            await SendAsync(eventMessage);
        }

        /// <summary>
        /// Send EventMessage to service bus
        /// </summary>
        /// <typeparam name="TMessage">Type of the object to send as a message.</typeparam>
        /// <param name="message">object to send to service bus.</param>
        /// <returns></returns>
        internal async Task SendAsync<TMessage>(TMessage message)
        {
            EventMessage eventMessage = new EventMessage();
            string body = null;
            if (MessageSerializer != null)
            {
                body = MessageSerializer.SerializeObjectToJson(message);
            }
            else
            {
                body = JsonConvert.SerializeObject(message);
            }
            eventMessage.Body = body;
            eventMessage.ObjectName = message.GetType().AssemblyQualifiedName;
            await SendAsync(eventMessage);
        }

        /// <summary>
        /// Send EventMessage to service bus
        /// </summary>
        /// <param name="eventMessage">message to send</param>
        /// <returns></returns>
        internal async Task SendAsync(EventMessage eventMessage)
        {
#if NETCOREAPP3_1 || NET5_0
            Message brokeredMessage = null;
#endif
#if NET6_0 || NET7_0
            ServiceBusMessage brokeredMessage = null;
#endif
            MessageFactory messageFactory;
            VeryLargeMessageStrategy strategy;
            if (CurrentSettingData != null)
            {
                strategy = CurrentSettingData.Strategy;
                messageFactory = new MessageFactory(CurrentSettingData);
            }
            else
            {
                strategy = Settings.GetVeryLargeMessageStrategy();
                messageFactory = new MessageFactory();
            }
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

        /// <summary>
        /// Send message to service bus using standard client
        /// </summary>
        /// <param name="message">message for queue or topic</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        protected async Task ToSendAsync(Message message)
        {
            TClient client = GetClient();
            await client.SendAsync(message);
        }
#endif
#if NET6_0 || NET7_0
        protected async Task ToSendAsync(ServiceBusMessage message)
        {
            TClient client = GetClient();
            await client.SendMessageAsync(message);
        }
#endif

        /// <summary>
        /// Send messagea to service bus using standard client
        /// </summary>
        /// <param name="messages">list of messages for queue or topic</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        protected async Task ToSendBatchAsync(List<Message> messages)
        {
            TClient client = GetClient();
            await client.SendAsync(messages);
        }
#endif
#if NET6_0 || NET7_0
        protected async Task ToSendBatchAsync(List<ServiceBusMessage> messages)
        {
            TClient client = GetClient();
            await client.SendMessagesAsync(messages);
        }
#endif

    }
}
