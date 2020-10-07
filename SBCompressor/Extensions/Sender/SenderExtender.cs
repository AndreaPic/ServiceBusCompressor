using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
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
        where TClient : IClientEntity, ISenderClient 
    {
        /// <summary>
        /// Current client of queue or topic
        /// </summary>
        private TClient Client { get; set; }

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
            string body = JsonConvert.SerializeObject(message);
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
            Message brokeredMessage = null;
            MessageFactory messageFactory = new MessageFactory();
            VeryLargeMessageStrategy strategy;
            if (CurrentSettingData != null)
            {
                strategy = CurrentSettingData.Strategy;
            }
            else
            {
                strategy = Settings.GetVeryLargeMessageStrategy();
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
        protected async Task ToSendAsync(Message message)
        {
            TClient client = GetClient();
            await client.SendAsync(message);
        }

        /// <summary>
        /// Send messagea to service bus using standard client
        /// </summary>
        /// <param name="messages">list of messages for queue or topic</param>
        /// <returns></returns>
        protected async Task ToSendBatchAsync(List<Message> messages)
        {
            TClient client = GetClient();
            await client.SendAsync(messages);
        }

    }
}
