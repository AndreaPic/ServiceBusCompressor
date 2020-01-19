using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Sender
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.QueueClient
    /// </summary>
    public static class QueueClientSenderExtensions
    {
           
        /// <summary>
        /// Send message to the queue
        /// </summary>
        /// <param name="queueClient">type to extend</param>
        /// <param name="message">Message for the queue (can be json)</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync(this IQueueClient queueClient, string message)
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient);
            await queueConnector.SendAsync(message);
        }

        /// <summary>
        /// Send message to the queue
        /// </summary>
        /// <typeparam name="TMessage">Type of the object to send as a message.</typeparam>
        /// <param name="queueClient">type to extend</param>
        /// <param name="message"></param>
        /// <param name="message">object to send to service bus.</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync<TMessage>(this IQueueClient queueClient, TMessage message)
            where TMessage : class, new()
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient);
            await queueConnector.SendAsync(message);
        }
    }
}
