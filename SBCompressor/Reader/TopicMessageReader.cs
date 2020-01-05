﻿using Microsoft.Azure.ServiceBus;
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
