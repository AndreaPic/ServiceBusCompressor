using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressorTests
{
    [TestClass]
    public class C_TopicSenderTests
    {
        const string ServiceBusConnectionStringName = "TopicConnectionString";
        const string TopicName = "<your_topic_name>";
        static TopicConnector topicClient;

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
            topicClient = new TopicConnector(TopicName, ServiceBusConnectionStringName);
        }

        [ClassCleanup]
        static public void Cleanup()
        {
            Task.Run(() => topicClient.CloseAsync()).Wait();
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
                await topicClient.SendAsync(message);
            }
            catch (Exception ex)
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
                message.Body = ResourceMessage.MessageToZip;
                await topicClient.SendAsync(message);
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
                message.Body = ResourceMessage.VeryLargeMessage;
                await topicClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }

    }
}
