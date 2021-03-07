using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;
using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using SBCompressor.Extensions.Reader;
using SBCompressor;

namespace FunctionAppTest
{
    public static class FunctionTest
    {

        //Microsoft.Azure.WebJobs.ServiceBus.
        //[FunctionName("Function1")]
        //public static Book Run([QueueTrigger("functionstesting2", Connection = "AzureWebJobsStorage")] Book myQueueItem,
        //    [Blob("test-samples/sample1.txt", Connection = "AzureWebJobsStorage")] string myBlob)
        //{
        //    Console.WriteLine(myBlob);
        //    return myQueueItem;
        //}

        //public class Book
        //{
        //    public string name { get; set; }
        //    public string id { get; set; }
        //}

        [FunctionName("FunctionReceiver")]
        public static void Run([ServiceBusTrigger("crashqueue", Connection = "ConnectionStringName")]
            string myQueueItem,
            Microsoft.Azure.Functions.Worker.Pipeline.FunctionExecutionContext executionContext,
            Dictionary<string, object> UserProperties,
            Microsoft.Azure.ServiceBus.Message message)
            //MessageReceiver messageReceiver,
            //IDictionary<string,object> userProperties)
            //Microsoft.Azure.ServiceBus.Message message)
            //string myQueueItem,
            //Int32 deliveryCount,
            //DateTime enqueuedTimeUtc,
            //string messageId)
            //string message, int deliveryCount,
            //MessageReceiver messageReceiver,
            //Message msg,
            //string lockToken)
        {            
            Console.WriteLine($"C# ServiceBus queue trigger function processed message: Hello");
            //log.LogInformation($"C# ServiceBus queue trigger function processed message: {myMessage}");
            FunctionMessageReader reader = new FunctionMessageReader();
            reader.SubScribe(message, ProcessMessages);
        }
        private static void ProcessMessages(MessageReceivedEventArgs e)
        {
            // Process the message.
            if (!string.IsNullOrEmpty(e.ReceivedEventMessage.Body))
            {
                Console.WriteLine($"Received message: SequenceNumber:{e.ReceivedMessage.SystemProperties.SequenceNumber} Body:{e.ReceivedEventMessage.Body}");
            }
            else
            {
                if (typeof(DTOLibrary.MessageDTO).AssemblyQualifiedName == e.ReceivedEventMessage.ObjectName)
                {
                    DTOLibrary.MessageDTO msgDTO = e.ReceivedEventMessage.BodyObject as DTOLibrary.MessageDTO;
                    if (msgDTO != null)
                    {
                        Console.WriteLine(msgDTO.Subject + " " + msgDTO.Content);
                    }
                }
            }
        }

    }
}
