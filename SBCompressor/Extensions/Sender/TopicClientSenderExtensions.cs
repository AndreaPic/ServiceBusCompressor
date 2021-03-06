﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using SBCompressor.Configuration;
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
        public static async Task SendCompressorAsync(this ITopicClient topicClient,
            string message, StorageSettingData settingData)
        {
            SenderExtender<ITopicClient> topicConnector = new SenderExtender<ITopicClient>(topicClient, settingData);
            await topicConnector.SendAsync(message);
        }

        /// <summary>
        /// Send message to the Topic
        /// </summary>
        /// <typeparam name="TMessage">Type of the object to send as a message.</typeparam>
        /// <param name="queueClient">type to extend</param>
        /// <param name="message"></param>
        /// <param name="message">object to send to service bus.</param>
        /// <returns></returns>
        public static async Task SendCompressorAsync<TMessage>(this ITopicClient queueClient, TMessage message)
            where TMessage : class, new()
        {
            SenderExtender<ITopicClient> queueConnector = new SenderExtender<ITopicClient>(queueClient);
            await queueConnector.SendAsync(message);
        }
        public static async Task SendCompressorAsync<TMessage>(this ITopicClient queueClient, TMessage message, StorageSettingData settingData)
            where TMessage : class, new()
        {
            SenderExtender<ITopicClient> queueConnector = new SenderExtender<ITopicClient>(queueClient, settingData);
            await queueConnector.SendAsync(message);
        }

    }
}
