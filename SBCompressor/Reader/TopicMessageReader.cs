using Microsoft.Azure.ServiceBus;
using SBCompressor.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SBCompressor
{
    /// <summary>
    /// Topic client for reading operations
    /// </summary>
    public class TopicMessageReader : BaseMessageReader<SubscriptionClient>
    {
        /// <summary>
        /// name of the subscription for the topic
        /// </summary>
        private string subscriptionName;
        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="subscriptionName">Subscription name for the topic</param>
        public TopicMessageReader(string entityName, string connectionStringName, string subscriptionName) : base(entityName, connectionStringName)
        {
            this.subscriptionName = subscriptionName;
        }
        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="subscriptionName">Subscription name for the topic</param>
        /// <param name="settingData">Explicit Settings</param>
        public TopicMessageReader(string entityName, string connectionStringName, string subscriptionName,
            StorageSettingData settingData) : base(entityName, connectionStringName, settingData)
        {
            this.subscriptionName = subscriptionName;
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="subscriptionName">Subscription name for the topic</param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        public TopicMessageReader(string entityName, string connectionStringName, string subscriptionName, Type typeToDeserialize) : base(entityName, connectionStringName, typeToDeserialize)
        {
            this.subscriptionName = subscriptionName;
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="subscriptionName">Subscription name for the topic</param>
        /// <param name="deserializer">Object used to deserialize message</param>
        public TopicMessageReader(string entityName, string connectionStringName, string subscriptionName, IMessageDeserializer deserializer) : base(entityName, connectionStringName, deserializer)
        {
            this.subscriptionName = subscriptionName;
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="subscriptionName">Subscription name for the topic</param>
        /// <param name="settingData">Explicit Settings</param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        public TopicMessageReader(string entityName, string connectionStringName, string subscriptionName,
            StorageSettingData settingData, Type typeToDeserialize) : base(entityName, connectionStringName, settingData, typeToDeserialize)
        {
            this.subscriptionName = subscriptionName;
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="subscriptionName">Subscription name for the topic</param>
        /// <param name="settingData">Explicit Settings</param>
        /// <param name="deserializer">Object used to deserialize message</param>
        public TopicMessageReader(string entityName, string connectionStringName, string subscriptionName,
            StorageSettingData settingData, IMessageDeserializer deserializer) : base(entityName, connectionStringName, settingData, deserializer)
        {
            this.subscriptionName = subscriptionName;
        }

        /// <summary>
        /// Create a new instance of SubscriptionClient
        /// </summary>
        /// <returns>SubscriptionClient used for communicaiton with service bus</returns>
        protected override SubscriptionClient CreateSubscriptionClient()
        {
            var client = new SubscriptionClient(ServiceBusConnectionString, EntityName, subscriptionName);
            return client;
        }

    }
}
