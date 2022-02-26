#if NET6_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
#endif
using SBCompressor.Configuration;
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
#if NETCOREAPP3_1 || NET5_0
        public static void SubscribeCompressor(this IQueueClient queueClient, 
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            ReaderExtender<IQueueClient> reader = new ReaderExtender<IQueueClient>(queueClient);
            reader.Subscribe(onMessageReceived);
        }
#endif
#if NET6_0
        public static void SubscribeCompressor(this ServiceBusReceiver queueClient,
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            ReaderExtender<ServiceBusReceiver> reader = new ReaderExtender<ServiceBusReceiver>(queueClient);
            reader.Subscribe(onMessageReceived);
        }
#endif
        /// <summary>
        /// Subscribe action to read queue messages
        /// </summary>
        /// <param name="queueClient">Type to extend</param>
        /// <param name="onMessageReceived">Action invoked when message arrive</param>
        /// <param name="settingData">Setting infomrations</param>
#if NETCOREAPP3_1 || NET5_0
        public static void SubscribeCompressor(this IQueueClient queueClient,
            Action<MessageReceivedEventArgs> onMessageReceived, StorageSettingData settingData)
        {
            ReaderExtender<IQueueClient> reader = new ReaderExtender<IQueueClient>(queueClient, settingData);
            reader.Subscribe(onMessageReceived);
        }
#endif
#if NET6_0
        public static void SubscribeCompressor(this ServiceBusReceiver queueClient,
            Action<MessageReceivedEventArgs> onMessageReceived, StorageSettingData settingData)
        {
            ReaderExtender<ServiceBusReceiver> reader = new ReaderExtender<ServiceBusReceiver>(queueClient, settingData);
            reader.Subscribe(onMessageReceived);
        }
#endif

    }
}
