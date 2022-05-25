#if NET6_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
#endif
using SBCompressor.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Sender
{
    /// <summary>
    /// Extensions for use this library functionality directly form Microsoft.Azure.ServiceBus.QueueClient
    /// </summary>
#if NETCOREAPP3_1 || NET5_0
    public static class QueueClientSenderExtensions
#endif
#if NET6_0
    static class QueueClientSenderExtensions
#endif
    {

        /// <summary>
        /// Send message to the queue
        /// </summary>
        /// <param name="queueClient">type to extend</param>
        /// <param name="message">Message for the queue (can be json)</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        public static async Task SendCompressorAsync(this IQueueClient queueClient, string message)
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient);
            await queueConnector.SendAsync(message);
        }
#endif
#if NET6_0
        public static async Task SendCompressorAsync(this ServiceBusSender queueClient, string message)
        {
            SenderExtender<ServiceBusSender> queueConnector = new SenderExtender<ServiceBusSender>(queueClient);
            await queueConnector.SendAsync(message);
        }
#endif
#if NETCOREAPP3_1 || NET5_0
        public static async Task SendCompressorAsync(this IQueueClient queueClient, string message, StorageSettingData settingData)
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient, settingData);
            await queueConnector.SendAsync(message);
        }
#endif
#if NET6_0
        public static async Task SendCompressorAsync(this ServiceBusSender queueClient, string message, StorageSettingData settingData)
        {
            SenderExtender<ServiceBusSender> queueConnector = new SenderExtender<ServiceBusSender>(queueClient, settingData);
            await queueConnector.SendAsync(message);
        }
#endif

        /// <summary>
        /// Send message to the queue
        /// </summary>
        /// <typeparam name="TMessage">Type of the object to send as a message.</typeparam>
        /// <param name="queueClient">type to extend</param>
        /// <param name="message"></param>
        /// <param name="message">object to send to service bus.</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        public static async Task SendCompressorAsync<TMessage>(this IQueueClient queueClient, TMessage message)
            where TMessage : class, new()
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient);
            await queueConnector.SendAsync(message);
        }
#endif
#if NET6_0
        public static async Task SendCompressorAsync<TMessage>(this ServiceBusSender queueClient, TMessage message)
            where TMessage : class, new()
        {
            SenderExtender<ServiceBusSender> queueConnector = new SenderExtender<ServiceBusSender>(queueClient);
            await queueConnector.SendAsync(message);
        }
#endif

        /// <summary>
        /// Send message to the queue
        /// </summary>
        /// <typeparam name="TMessage">Type of the object to send as a message.</typeparam>
        /// <param name="queueClient">type to extend</param>
        /// <param name="message"></param>
        /// <param name="serializer">Object used to serialize message</param>
        /// <returns></returns>
#if NETCOREAPP3_1 || NET5_0
        public static async Task SendCompressorAsync<TMessage>(this IQueueClient queueClient, TMessage message, IMessageSerializer serializer)
            where TMessage : class, new()
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient, serializer);
            await queueConnector.SendAsync(message);
        }
#endif
#if NET6_0
        public static async Task SendCompressorAsync<TMessage>(this ServiceBusSender queueClient, TMessage message, IMessageSerializer serializer)
            where TMessage : class, new()
        {
            SenderExtender<ServiceBusSender> queueConnector = new SenderExtender<ServiceBusSender>(queueClient, serializer);
            await queueConnector.SendAsync(message);
        }
#endif

#if NETCOREAPP3_1 || NET5_0
        public static async Task SendCompressorAsync<TMessage>(this IQueueClient queueClient, TMessage message, StorageSettingData settingData)
            where TMessage : class, new()
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient, settingData);
            await queueConnector.SendAsync(message);
        }
#endif
#if NET6_0
        public static async Task SendCompressorAsync<TMessage>(this ServiceBusSender queueClient, TMessage message, StorageSettingData settingData)
            where TMessage : class, new()
        {
            SenderExtender<ServiceBusSender> queueConnector = new SenderExtender<ServiceBusSender>(queueClient, settingData);
            await queueConnector.SendAsync(message);
        }
#endif


#if NETCOREAPP3_1 || NET5_0
        public static async Task SendCompressorAsync<TMessage>(this IQueueClient queueClient, TMessage message, StorageSettingData settingData, IMessageSerializer serializer)
            where TMessage : class, new()
        {
            SenderExtender<IQueueClient> queueConnector = new SenderExtender<IQueueClient>(queueClient, settingData, serializer);
            await queueConnector.SendAsync(message);
        }
#endif
#if NET6_0
        public static async Task SendCompressorAsync<TMessage>(this ServiceBusSender queueClient, TMessage message, StorageSettingData settingData, IMessageSerializer serializer)
            where TMessage : class, new()
        {
            SenderExtender<ServiceBusSender> queueConnector = new SenderExtender<ServiceBusSender>(queueClient, settingData, serializer);
            await queueConnector.SendAsync(message);
        }
#endif

    }
}
