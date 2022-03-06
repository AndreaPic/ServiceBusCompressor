using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SBCompressor;
using SBCompressor.Configuration;
using SBCompressor.Extensions.Reader;

namespace FunctionAppTest6
{
    public static class FunctionTest
    {
        [FunctionName("FunctionTest")]
        public static void Run([ServiceBusTrigger(
                                "<your_queue_name>",
                                Connection = "QueueConnectionString")]
            byte[] item,
            Int32 deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
            IDictionary<string, object> UserProperties,
            ILogger log)
            //Azure.Messaging.ServiceBus.ServiceBusReceivedMessage message , ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {messageId}");
            FunctionInputData functionInputData = new FunctionInputData(item, messageId, UserProperties);

            var settings =
                new StorageSettingData(
                    "<your_blob_storage_container_name>",
                    "<your_blob_storage_connection_string>",
                    VeryLargeMessageStrategy.Storage);
            FunctionMessageReader reader = new FunctionMessageReader(settings);

            reader.SubScribe(functionInputData,
                (arg) =>
                {
                    //PUT HERE YOUR MESSAGE HANDLING LOGIC
                    log.LogInformation($"function processed object: {arg.ReceivedEventMessage?.ObjectName ?? arg.ReceivedEventMessage.Body.Substring(0, 10) }");
                }
                );
        }
    }
}
