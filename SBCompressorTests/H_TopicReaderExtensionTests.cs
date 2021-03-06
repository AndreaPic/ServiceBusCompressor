﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SBCompressor.Extensions.Reader;
using Microsoft.Azure.ServiceBus;

namespace SBCompressorTests
{
    [TestClass]
    public class H_TopicReaderExtensionTests
    {
        const string ServiceBusConnectionString = "<your_connection_string>";
        const string TopicName = "<your_topic_name>";
        const string SubscriptionName = "<your_subscription_name>";
        static ISubscriptionClient subscriptionClient;

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
            subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName, SubscriptionName);
        }

        [ClassCleanup]
        static public void Cleanup()
        {
            Task.Run(() => subscriptionClient.CloseAsync()).Wait();
        }

        private const int minReceivedMessage = 3;
        private int receivedMessageCounter = 0;
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        [TestMethod]
        public void TestReadMessages()
        {
            receivedMessageCounter = 0;
            subscriptionClient.SubscribeCompressor(QueueClient_OnMessageReceived);
            autoResetEvent.WaitOne(30000);
            Assert.IsTrue(receivedMessageCounter >= minReceivedMessage);
        }
        private void QueueClient_OnMessageReceived(MessageReceivedEventArgs e)
        {
            receivedMessageCounter++;
            if (receivedMessageCounter >= minReceivedMessage)
            {
                autoResetEvent.Set();
            }
        }


    }
}
