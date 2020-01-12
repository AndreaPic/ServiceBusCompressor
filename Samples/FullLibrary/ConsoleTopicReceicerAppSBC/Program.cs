using SBCompressor;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTopicReceicerAppSBC
{
    class Program
    {
        const string ServiceBusConnectionName = "receiverFromTopicConnectionString";
        const string TopicName = "<your_topic_name>";
        const string SubscriptionName = "<your_topic_subscription_name>";
        static TopicMessageReader subscriptionClient;

        public static async Task Main(string[] args)
        {
            subscriptionClient = new TopicMessageReader(TopicName, ServiceBusConnectionName, SubscriptionName);

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register subscription message handler and receive messages in a loop
            await RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await subscriptionClient.CloseAsync();
        }

        static async Task RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Register the action that processes messages.
            await subscriptionClient.EnsureSubscribe(SubscriptionClient_OnMessageReceived);
        }

        private static void SubscriptionClient_OnMessageReceived(MessageReceivedEventArgs e)
        {
            // Process the message.
            var message = e.ReceivedMessage;
            var eventMessage = e.ReceivedEventMessage;
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{eventMessage.Body}");
        }

    }
}
