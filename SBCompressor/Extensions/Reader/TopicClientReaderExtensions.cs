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
using SBCompressor.Extensions.Reader;

namespace SBCompressor.Extensions.TopicReader
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.TopicClient
    /// </summary>
    public static class TopicClientReaderExtensions
    {
        /// <summary>
        /// Subscribe action to read queue messages
        /// </summary>
        /// <param name="topicClient">type to extend</param>
        /// <param name="topicName">Topic's name for the message</param>
        /// <param name="serviceBusConnectionStringName">Topic connection string name (must be present in sbcsettings.json file)</param>
        /// <param name="onMessageReceived">Action invoked when message arrive</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        public static void SubscribeCompressor(this ISubscriptionClient topicClient, 
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            ReaderExtender<ISubscriptionClient> topicMessageReader = new ReaderExtender<ISubscriptionClient>(topicClient);
            topicMessageReader.Subscribe(onMessageReceived);
        }
#endif
#if NET6_0
        //public static void SubscribeCompressor(this ServiceBusReceiver topicClient,
        //public static void SubscribeCompressor(this ServiceBusReceiver topicClient,
        //    Action<MessageReceivedEventArgs> onMessageReceived)
        public static void SubscribeCompressor(this ServiceBusProcessor topicClient,
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            ReaderExtender<ServiceBusProcessor> topicMessageReader = new ReaderExtender<ServiceBusProcessor>(topicClient);
            //ReaderExtender<ServiceBusReceiver> topicMessageReader = new ReaderExtender<ServiceBusReceiver>(topicClient);
            topicMessageReader.Subscribe(onMessageReceived);
        }
#endif

        /// <summary>
        /// Subscribe action to read queue messages
        /// </summary>
        /// <param name="topicClient">type to extend</param>
        /// <param name="onMessageReceived">Action invoked when message arrive</param>
        /// <param name="settingData">Setting infomrations</param>
#if NETCOREAPP3_1 || NET5_0
        public static void SubscribeCompressor(this ISubscriptionClient topicClient,
            Action<MessageReceivedEventArgs> onMessageReceived, StorageSettingData settingData)
        {
            ReaderExtender<ISubscriptionClient> topicMessageReader = new ReaderExtender<ISubscriptionClient>(topicClient, settingData);
            topicMessageReader.Subscribe(onMessageReceived);
        }
#endif
#if NET6_0
        //public static void SubscribeCompressor(this ServiceBusReceiver topicClient,
        //    Action<MessageReceivedEventArgs> onMessageReceived, StorageSettingData settingData)
        public static void SubscribeCompressor(this ServiceBusProcessor topicClient,
            Action<MessageReceivedEventArgs> onMessageReceived, StorageSettingData settingData)
        {
            ReaderExtender<ServiceBusProcessor> topicMessageReader = new ReaderExtender<ServiceBusProcessor>(topicClient, settingData);
            //ReaderExtender<ServiceBusReceiver> topicMessageReader = new ReaderExtender<ServiceBusReceiver>(topicClient, settingData);
            topicMessageReader.Subscribe(onMessageReceived);
        }
#endif

    }
}
