﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBCompressorTests
{
    [TestClass]
    public class B_QueueReaderTests
    {
        const string ServiceBusConnectionStringName = "QueueConnectionString";
        const string QueueName = "<your_queue_name>";
        static QueueMessageReader queueClient;

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
            queueClient = new QueueMessageReader(QueueName, ServiceBusConnectionStringName);
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
        public async Task TestReadMessages()
        {
            receivedMessageCounter = 0;
            await queueClient.EnsureSubscribe(QueueClient_OnMessageReceived);
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
