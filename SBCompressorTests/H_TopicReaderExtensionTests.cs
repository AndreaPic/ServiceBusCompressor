using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SBCompressor.Extensions.TopicReader;
using SBCompressor.Configuration;
#if NET6_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif

namespace SBCompressorTests
{
    [TestClass]
    public class H_TopicReaderExtensionTests
    {
        static string ServiceBusConnectionString = SBCSettings.ServiceBusConnectionString;
        const string TopicName = "sbt-testunitmessage";
        const string SubscriptionName = "testunitmessage-testclient";

#if NETCOREAPP3_1 || NET5_0
        static ISubscriptionClient subscriptionClient;
#endif
#if NET6_0
        //static ServiceBusReceiver subscriptionClient;
        static ServiceBusProcessor subscriptionClient;
#endif

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
#if NETCOREAPP3_1 || NET5_0
            subscriptionClient = new SubscriptionClient(ServiceBusConnectionString, TopicName, SubscriptionName);
#endif
#if NET6_0
            var sbClient = new ServiceBusClient(ServiceBusConnectionString);
            //subscriptionClient = sbClient.CreateReceiver(TopicName, SubscriptionName);
            subscriptionClient = sbClient.CreateProcessor(TopicName, SubscriptionName, new ServiceBusProcessorOptions() { MaxConcurrentCalls = 1 });            
#endif
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
#if NET6_0
            subscriptionClient.StartProcessingAsync();
#endif

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
