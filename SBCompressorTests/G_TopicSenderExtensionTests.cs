using Microsoft.VisualStudio.TestTools.UnitTesting;
using SBCompressor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SBCompressor.Extensions.Sender;
using Microsoft.Azure.ServiceBus;

namespace SBCompressorTests
{
    [TestClass]
    public class G_TopicSenderExtensionTests
    {
        const string ServiceBusConnectionString = "<your_connection_string>";
        const string TopicName = "<your_topic_name>";
        static ITopicClient topicClient;

        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
            topicClient = new TopicClient(ServiceBusConnectionString, TopicName);
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
                await topicClient.SendCompressorAsync(messageBody);
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
                await topicClient.SendCompressorAsync(ResourceMessage.large);
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
                await topicClient.SendCompressorAsync(ResourceMessage.verylarge);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }

    }
}
