using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SBCompressor;
using SBCompressor.Configuration;
using SBCompressor.Extensions.Reader;

namespace FunctionAppTest31
{
    public static class FunctionTest
    {
        [FunctionName("FunctionTest")]
        public static void Run([ServiceBusTrigger(
                                "<your_queue_name>", 
                                Connection = "QueueConnectionString")]
                                Microsoft.Azure.ServiceBus.Message message,
                                Int32 deliveryCount,
                                DateTime enqueuedTimeUtc,
                                string messageId,
                                ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed messageId: {message.MessageId} delivery count: {deliveryCount}");
            var settings = 
                new StorageSettingData(
                    "<your_blob_storage_container_name>",
                    "<your_blob_storage_connection_string>", 
                    VeryLargeMessageStrategy.Storage);
            FunctionMessageReader reader = new FunctionMessageReader(settings);

            reader.SubScribe(message, 
                (arg) => 
                {
                    //PUT HERE YOUR MESSAGE HANDLING LOGIC
                    log.LogInformation($"function processed object: {arg.ReceivedEventMessage?.ObjectName?? arg.ReceivedEventMessage.Body}");                    
                }
                );
        }
    }
}
