using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Threading.Tasks;

namespace SBCompressorTests
{
#if NETCOREAPP3_1 || NET5_0
    [TestClass]
    public class A_QueueSenderTests
    {
        const string ServiceBusConnectionStringName = "QueueConnectionString";
        const string QueueName = "<your_queue_name>";
        private static QueueConnector queueClient;

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            queueClient = new QueueConnector(QueueName, ServiceBusConnectionStringName);
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
                string messageBody = "Hello Message";
                EventMessage message = new EventMessage();
                message.Body = messageBody;
                await queueClient.SendAsync(message);
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
                EventMessage message = new EventMessage();
                message.Body = ResourceMessage.large;
                await queueClient.SendAsync(message);
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
                EventMessage message = new EventMessage();
                message.Body = ResourceMessage.VeryLargeMsg;
                await queueClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }

    }
#endif
}
