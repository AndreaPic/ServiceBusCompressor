using Microsoft.Azure.ServiceBus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Threading.Tasks;
using SBCompressor.Extensions.Sender;

namespace SBCompressorTests
{
    [TestClass]
    public class E_QueueSenderExtensionTests
    {
        const string ServiceBusConnectionString = "<your_connection_string>";
        const string QueueName = "<your_queue_name>";
        static QueueClient queueClient;
        const int numberOfMessages = 10;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
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
                await queueClient.SendCompressorAsync(ResourceMessage.verylarge);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }

    }
}
