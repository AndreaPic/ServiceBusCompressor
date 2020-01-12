using Microsoft.Azure.ServiceBus;
using SBCompressor;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleReceiverAppSBC
{
    class Program
    {
        const string ServiceBusConnectionStringName = "receiverFromQueueConnectionString";
        const string QueueName = "<your_queue_name>";
        static QueueMessageReader queueClient;

        public static async Task Main(string[] args)
        {
            queueClient = new QueueMessageReader(QueueName, ServiceBusConnectionStringName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register the queue message handler and receive messages in a loop
            await RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await queueClient.CloseAsync();
        }
        static async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Register the action that processes messages.
            await queueClient.EnsureSubscribe(QueueClient_OnMessageReceived);
        }

        // Process the message.
        private static void QueueClient_OnMessageReceived(MessageReceivedEventArgs e)
        {
            var message = e.ReceivedMessage;
            var eventMessage = e.ReceivedEventMessage;
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{eventMessage.Body}");
        }

    }
}
