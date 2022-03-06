// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SBCompressor;
using SBCompressor.Configuration;
using SBCompressor.Extensions;
using SBCompressor.Extensions.Reader;

namespace SampleApp
{

    //LAUNCH WITH "func host start" command in the bin directory

    public static class ServiceBusFunction
    {
        [Function("ServiceBusFunction")]
        public static async Task Run([ServiceBusTrigger(
            "<your_queue_name>",
            Connection = "QueueConnectionString")] 
            byte[] item,
            Int32 deliveryCount,
            DateTime enqueuedTimeUtc,
            string messageId,
            IDictionary<string, object> UserProperties,
            FunctionContext context)
        {

            var logger = context.GetLogger("ServiceBusFunction");
            logger.LogInformation($"MessageID: {messageId}");
            FunctionInputData functionInputData = new FunctionInputData(item, messageId, UserProperties);

            var settings =
                new StorageSettingData(
                    "<your_blob_storage_container_name>",
                    "<your_blob_storage_connection_string>",
                    VeryLargeMessageStrategy.Storage);
            FunctionMessageReader reader = new FunctionMessageReader(settings);

            await reader.SubScribe(functionInputData,
                (arg) =>
                {
                    //PUT HERE YOUR MESSAGE HANDLING LOGIC
                    logger.LogInformation($"function processed object: {arg.ReceivedEventMessage?.ObjectName??arg.ReceivedEventMessage.Body.Substring(0,10)}");
                });
            return;
        }
    }
}
