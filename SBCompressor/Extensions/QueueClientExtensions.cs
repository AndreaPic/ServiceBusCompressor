using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.QueueClient
    /// </summary>
    public static class QueueClientExtensions
    {
        /// <summary>
        /// Send message to the queue
        /// </summary>
        /// <param name="queueClient">type to extend</param>
        /// <param name="queueName">Queue's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Queue connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="message">Message for the queue (can be json)</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync(this QueueClient queueClient,
            string queueName,
            string serviceBusConnectionStringName,
            string message)
        {
            QueueConnector queueConnector = new QueueConnector(queueName, serviceBusConnectionStringName);
            await queueConnector.SendAsync(message);
        }

        /// <summary>
        /// Subscribe action to read queue messages
        /// </summary>
        /// <param name="queueClient">type to extend</param>
        /// <param name="queueName">Queue's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Queue connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="onMessageReceived">Action invoked when message arrive</param>
        /// <returns></returns>
        public static async Task SubscribeCompressor(this QueueClient queueClient, 
            string queueName, 
            string serviceBusConnectionStringName, 
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            var queueMessageReader = new QueueMessageReader(queueName, serviceBusConnectionStringName);
            await queueMessageReader.EnsureSubscribe(onMessageReceived);
        }
        /// <summary>
        /// Close the Client
        /// </summary>
        /// <param name="queueClient">Type to extend</param>
        /// <param name="queueName">Queue name for the client to close</param>
        /// <returns>True if closed successfully</returns>
        public static async Task<bool> CloseSenderCompressor(this QueueClient queueClient,
            string queueName)
        {
            return await BaseConnector<QueueClient>.CloseAsync(queueName);
        }

    }
}
