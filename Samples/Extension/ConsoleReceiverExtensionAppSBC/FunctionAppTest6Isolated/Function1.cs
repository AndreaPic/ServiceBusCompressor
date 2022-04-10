using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SBCompressor;
using SBCompressor.Configuration;
using SBCompressor.Extensions.Reader;

namespace FunctionAppTest6Isolated
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Function1>();
        }

        [Function("Function1")]
        public void Run([ServiceBusTrigger("sbq-testunitmessage", Connection = "QueueConnectionString")] byte[] item,
            string messageId,
            IDictionary<string, object> UserProperties)
        {
            _logger.LogInformation($"C# ServiceBus queue trigger function processed message: {messageId}");
            FunctionInputData functionInputData = new FunctionInputData(item, messageId, UserProperties);

            var settings =
                new StorageSettingData(
                    "<your storage container name>",
                    "<blob storage connection string>",
                    VeryLargeMessageStrategy.Storage);
            FunctionMessageReader reader = new FunctionMessageReader(settings);

            reader.SubScribe(functionInputData,
                (arg) =>
                {
                    //PUT HERE YOUR MESSAGE HANDLING LOGIC
                    _logger.LogInformation($"function processed object: {arg.ReceivedEventMessage?.ObjectName ?? arg.ReceivedEventMessage.Body.Substring(0, 10) }");
                }
                );

        }
    }
}
