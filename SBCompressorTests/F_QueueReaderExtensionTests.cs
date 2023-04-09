#if NET6_0 || NET7_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SBCompressor.Extensions.QueueReader;
using SBCompressor.Configuration;

namespace SBCompressorTests
{
    [TestClass]
    public class F_QueueReaderExtensionTests
    {
        static string ServiceBusConnectionString = SBCSettings.ServiceBusConnectionString;
        const string QueueName = "sbq-testunitmessage";
#if NETCOREAPP3_1 || NET5_0
        static IQueueClient queueClient;
#endif
#if NET6_0 || NET7_0
        //static ServiceBusReceiver queueClient;
        static ServiceBusProcessor queueClient;
#endif

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
#if NETCOREAPP3_1 || NET5_0
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
#endif
#if NET6_0 || NET7_0
            var sbClient = new ServiceBusClient(ServiceBusConnectionString);
            //queueClient = sbClient.CreateReceiver(QueueName);
            queueClient = sbClient.CreateProcessor(QueueName,new ServiceBusProcessorOptions() {  MaxConcurrentCalls = 1});
#endif
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
#if NETCOREAPP3_1 || NET5_0
        public void TestReadMessages()
#endif
#if NET6_0 || NET7_0
        public async Task TestReadMessages()
#endif
        {
            receivedMessageCounter = 0;            
            queueClient.SubscribeCompressor(QueueClient_OnMessageReceived);
#if NET6_0 || NET7_0
            await queueClient.StartProcessingAsync();
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
