using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SBCompressor.Extensions.Reader;

namespace SBCompressorTests
{
    [TestClass]
    public class F_QueueReaderExtensionTests
    {
        const string ServiceBusConnectionString = "<your_connection_string>";
        const string QueueName = "<your_queue_name>";
        static IQueueClient queueClient;

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
        }

        [ClassCleanup]
        static public void Cleanup()
        {
            Task.Run(() => queueClient.CloseAsync()).Wait();
        }

        private const int minReceivedMessage = 3;
        private int receivedMessageCounter = 0;
        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        [TestMethod]
        public void TestReadMessages()
        {
            receivedMessageCounter = 0;
            queueClient.SubscribeCompressorAsync(QueueClient_OnMessageReceived);
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
