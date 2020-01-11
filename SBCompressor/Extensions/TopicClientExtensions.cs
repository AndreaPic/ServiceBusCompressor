using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.TopicClient
    /// </summary>
    public static class TopicClientExtensions
    {
        /// <summary>
        /// Send message to the topic
        /// </summary>
        /// <param name="topicClient">type to extend</param>
        /// <param name="topicName">Topic's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Topic connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="message">Message for the queue (can be json)</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync(this TopicClient topicClient,
            string topicName,
            string serviceBusConnectionStringName,
            string message)
        {
            QueueConnector queueConnector = new QueueConnector(topicName, serviceBusConnectionStringName);
            await queueConnector.SendAsync(message);
        }

        /// <summary>
        /// Subscribe action to read queue messages
        /// </summary>
        /// <param name="topicClient">type to extend</param>
        /// <param name="topicName">Topic's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Topic connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="onMessageReceived">Action invoked when message arrive</param>
        /// <returns></returns>
        public static async Task SubscribeCompressor(this TopicClient topicClient, 
            string topicName, 
            string serviceBusConnectionStringName, 
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            var topicMessageReader = new QueueMessageReader(topicName, serviceBusConnectionStringName);
            await topicMessageReader.EnsureSubscribe(onMessageReceived);
            await topicMessageReader.CloseAsync();
        }
        /// <summary>
        /// Close the Client
        /// </summary>
        /// <param name="topicClient">Type to extend</param>
        /// <param name="topicName">Topic name for the client to close</param>
        /// <returns>True if closed successfully</returns>
        public static async Task<bool> CloseSenderCompressor(this TopicClient topicClient,
            string topicName)
        {
            return await BaseConnector<TopicClient>.CloseAsync(topicName);
        }

    }
}
