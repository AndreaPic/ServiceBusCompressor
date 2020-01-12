using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using SBCompressor.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Reader
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.QueueClient
    /// </summary>
    public static class QueueClientReaderExtensions
    {          
        /// <summary>
        /// Subscribe action to read queue messages
        /// </summary>
        /// <param name="queueClient">type to extend</param>
        /// <param name="queueName">Queue's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Queue connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="onMessageReceived">Action invoked when message arrive</param>
        /// <returns></returns>
        public static void SubscribeCompressor(this IQueueClient queueClient, 
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            ReaderExtender<IQueueClient> reader = new ReaderExtender<IQueueClient>(queueClient);
            reader.Subscribe(onMessageReceived);
        }

    }
}
