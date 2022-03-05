using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBCompressorTests
{
#if NETCOREAPP3_1 || NET5_0
    [TestClass]
    public class D_TopicReaderTests
    {
        const string ServiceBusConnectionName = "TopicConnectionString";
        const string TopicName = "sbt-testunitmessage";
        const string SubscriptionName = "testunitmessage-testclient";

        static TopicMessageReader subscriptionClient;

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
            subscriptionClient = new TopicMessageReader(TopicName, ServiceBusConnectionName, SubscriptionName);
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
        public async Task TestReadMessages()
        {
            receivedMessageCounter = 0;
            await subscriptionClient.EnsureSubscribe(QueueClient_OnMessageReceived);
            autoResetEvent.WaitOne(30000);
            Assert.IsTrue(receivedMessageCounter >= minReceivedMessage);
        }
        private void QueueClient_OnMessageReceived(MessageReceivedEventArgs e)
        {
            receivedMessageCounter++;
            if (receivedMessageCounter >= minReceivedMessage)
            {
                Task.Run(() => subscriptionClient.CloseAsync()).Wait();
                autoResetEvent.Set();
            }
        }
    }
#endif
}
