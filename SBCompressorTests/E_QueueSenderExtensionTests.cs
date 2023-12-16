#if NET6_0 || NET7_0 || NET8_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Threading.Tasks;
using SBCompressor.Extensions.Sender;
using SBCompressor.Configuration;

namespace SBCompressorTests
{
    [TestClass]
    public class E_QueueSenderExtensionTests
    {
        static string ServiceBusConnectionString = SBCSettings.ServiceBusConnectionString;
        const string QueueName = "sbq-testunitmessage";
#if NET6_0 || NET7_0 || NET8_0
        static ServiceBusSender queueClient;
#endif
#if NETCOREAPP3_1 || NET5_0
        static QueueClient queueClient;
#endif
        const int numberOfMessages = 10;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
#if NET6_0 || NET7_0 || NET8_0
            var sbClient = new ServiceBusClient(ServiceBusConnectionString);
            queueClient = sbClient.CreateSender(QueueName);            
#endif
#if NETCOREAPP3_1 || NET5_0
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
#endif
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            Task.Run(() => queueClient.CloseAsync()).Wait();
        }

        [TestMethod]
        public async Task TestSendSimpleMessage()
        {
            Exception exception = null;
            try
            {
                // Create a new message to send to the queue.
                string messageBody = "Hello Message";

                // Send the message to the queue.
                await queueClient.SendCompressorAsync(messageBody);
            }
            catch(Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }
        [TestMethod]
        public async Task TestSendLargeMessage()
        {
            Exception exception = null;
            try
            {
                await queueClient.SendCompressorAsync(ResourceMessage.large);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }
        [TestMethod]
        public async Task TestSendVeryLargeMessage()
        {
            Exception exception = null;
            try
            {
                await queueClient.SendCompressorAsync(ResourceMessage.VeryLargeMsg);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }

    }
}
