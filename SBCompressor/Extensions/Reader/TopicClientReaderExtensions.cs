using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using SBCompressor.Configuration;
using SBCompressor.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Reader
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
        public static void SubscribeCompressor(this ISubscriptionClient topicClient, 
            Action<MessageReceivedEventArgs> onMessageReceived)
        {
            ReaderExtender<ISubscriptionClient> topicMessageReader = new ReaderExtender<ISubscriptionClient>(topicClient);
            topicMessageReader.Subscribe(onMessageReceived);
        }

        public static void SubscribeCompressor(this ISubscriptionClient topicClient,
            Action<MessageReceivedEventArgs> onMessageReceived, StorageSettingData settingData)
        {
            ReaderExtender<ISubscriptionClient> topicMessageReader = new ReaderExtender<ISubscriptionClient>(topicClient, settingData);
            topicMessageReader.Subscribe(onMessageReceived);
        }

    }
}
