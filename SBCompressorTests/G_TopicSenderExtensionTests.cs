#if NET6_0
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
using System.Threading.Tasks;
using SBCompressor.Extensions.Sender;

namespace SBCompressorTests
{
    [TestClass]
    public class G_TopicSenderExtensionTests
    {
        const string ServiceBusConnectionString = "<your_connection_string>";
        const string TopicName = "sbt-testunitmessage";
#if NETCOREAPP3_1 || NET5_0
        static ITopicClient topicClient;
#endif
#if NET6_0
        static ServiceBusSender topicClient;
#endif
        [ClassInitialize]
        static public void Initialize(TestContext context)
        {
#if NET6_0
            var sbClient = new ServiceBusClient(ServiceBusConnectionString);
            topicClient = sbClient.CreateSender(TopicName);
#endif
#if NETCOREAPP3_1 || NET5_0
            topicClient = new TopicClient(ServiceBusConnectionString, TopicName);
#endif
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
                await topicClient.SendCompressorAsync(ResourceMessage.VeryLargeMsg);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Assert.IsNull(exception);
        }

    }
}
