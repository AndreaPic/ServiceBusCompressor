using Microsoft.Azure.ServiceBus;
using SBCompressor;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSenderAppSBC
{
    class Program
    {
        const string ServiceBusConnectionStringName = "senderToQueueConnectionString";
        const string QueueName = "<your_queue_name>";
        static QueueConnector queueClient;
        public static async Task Main(string[] args)
        {
            queueClient = new QueueConnector(QueueName, ServiceBusConnectionStringName);

            const int numberOfMessages = 2;

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after sending all the messages.");
            Console.WriteLine("======================================================");

            // Send messages.
            await SendMessagesAsync(numberOfMessages);
            await SendLargeMessagesAsync(numberOfMessages);
            await SendVeryLargeMessagesAsync(numberOfMessages);            

            Console.ReadKey();

            await queueClient.CloseAsync();
        }
        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    EventMessage message = new EventMessage();
                    message.Body = messageBody;

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        static async Task SendLargeMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    EventMessage message = new EventMessage();
                    message.Body = ResourceMessage.MessageToZip;

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        static async Task SendVeryLargeMessagesAsync(int numberOfMessagesToSend)
        {
            try
            {
                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    EventMessage message = new EventMessage();
                    message.Body = ResourceMessage.VeryLargeMessage;

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

    }
}
