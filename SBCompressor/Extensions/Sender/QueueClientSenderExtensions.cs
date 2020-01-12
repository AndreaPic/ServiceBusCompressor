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
        /// <param name="queueName">Queue's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Queue connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="message">Message for the queue (can be json)</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync(this QueueClient queueClient, string message)
        {
            SenderExtender<QueueClient> queueConnector = new SenderExtender<QueueClient>(queueClient);
            await queueConnector.SendAsync(message);
        }

    }
}
