using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Sender
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.TopicClient
    /// </summary>
    public static class TopicClientSenderExtensions
    {
        /// <summary>
        /// Send message to the topic
        /// </summary>
        /// <param name="topicClient">type to extend</param>
        /// <param name="topicName">Topic's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Topic connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="message">Message for the queue (can be json)</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync(this ITopicClient topicClient,
            string message)
        {
            SenderExtender<ITopicClient> topicConnector = new SenderExtender<ITopicClient>(topicClient);
            await topicConnector.SendAsync(message);
        }
    }
}
