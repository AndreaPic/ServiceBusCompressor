# Samples description
Samples uses the nuget package of the library.
Samples are:
- ConsoleSenderAppSBC\ConsoleSenderAppSBC.sln (example that sends messages to Queue)
- ConsoleReceiverAppSBC\ConsoleReceiverAppSBC.sln (example that reads messages from Queue)
- ConsoleTopicSenderAppSBC\ConsoleTopicSenderAppSBC.sln (example that sends messages for Topic)
- ConsoleTopicReceicerAppSBC\ConsoleTopicReceicerAppSBC.sln (example that reads messages from Topic)

I recommend you to use the samples in this order: 
1. ConsoleSenderAppSBC.sln (example that sends messages to Queue)
2. ConsoleReceiverAppSBC.sln (example that reads messages from Queue)
3. ConsoleTopicSenderAppSBC.sln (example that sends messages for Topic)
4. ConsoleTopicReceicerAppSBC.sln (example that reads messages from Topic)

# How to configure the samples
There are 2 samples for Azure Queue and 2 samples for Azure Topic

## Queuse Samples
### ConsoleSenderAppSBC
This sample demonstrate how to send message to queue using this library. This Sample send a simple message, a large message and a very large message.

1. Configure Azure Queue
To create the queue follow the steps of the article: [Use Azure portal to create a Service Bus queue](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal#prerequisites).

2. Apply your Queue's configuration
In the sample there is the sbcsettings.json file below

```json
{
  "ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>",
    "senderToQueueConnectionString": "<your_servicebus_connection_string>"
  },
  "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```
**Replace <your_servicebus_connection_string> with queue's connection string.**
**Can you see your queue's connection string following these instructions: [Get the connection string](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal#get-the-connection-string)**

3. Crete Azure Blob Storage 
To create the Azure blob storage follow steps at this link: [Create an Azure Storage account article](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

4. Apply your Blob Storage configuration
**Replace <your_blob_storage_connection_string> with blob storage's connection string.**

**Can you see your queue's connection string following instructions at this link:** [View and copy a connection string](https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string#view-and-copy-a-connection-string)

5. Create Azure Blob Storage Container
To create a container follow steps at this link: [Create a container](https://docs.microsoft.com/it-it/azure/storage/blobs/storage-quickstart-blobs-portal#create-a-container)

**Replace <your_blob_storage_container_name> with blob storage's container name just created.**

6. Configure queue name in program.cs
in program.cs file look for <your_queue_name> and replace it with the queue name previously created using these instruction [Create a queue in the Azure portal](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-portal#create-a-queue-in-the-azure-portal)

```C#
class Program
    {
        const string ServiceBusConnectionStringName = "senderToQueueConnectionString";
        const string QueueName = "<your_queue_name>";
        static QueueConnector queueClient;
```

7. Run the Console Application

### ConsoleReceiverAppSBC
This sample demonstrate how read message from the queue. This Sample read messages sent by previous sample.

1. Configure Azure Queue
You must use the queue of the previous sample.

2. Apply your Queue's configuration
Open the solution ConsoleSenderAppSBC.sln
In the solution there is the sbcsettings.json file like this:

```json
{
  "ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>",
    "receiverFromQueueConnectionString": "<your_servicebus_connection_string>"
  },
  "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```
**Replace <your_servicebus_connection_string> with queue's connection string** (the same of previous sample).

3. Azure Blob Storage 
You must use the blob storage of the previous sample.

4. Apply your Blob Storage configuration
**Replace <your_blob_storage_connection_string> with blob storage's connection string** (the same of previous sample).

5. Apply Blob Storage Container Name
**Replace <your_blob_storage_container_name> with blob storage's container name** (the same of previous sample).

6. Configure queue name in program.cs
in program.cs file look for <your_queue_name> and replace it with the queue name of the previous sample.

```C#
class Program
    {
        const string ServiceBusConnectionStringName = "senderToQueueConnectionString";
        const string QueueName = "<your_queue_name>";
        static QueueConnector queueClient;
```

7. Run the Console Application

## Topic Samples
### ConsoleTopicSenderAppSBC
This sample demonstrate how to send message to a Topic using this library. This Sample send a simple message, a large message and a very large message.

1. Configure Azure Topic
To create the Topic follow the steps of the article: [Use the Azure portal to create a Service Bus topic and subscriptions to the topic](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-topics-subscriptions-portal#create-a-namespace-in-the-azure-portal).

2. Apply your Topic's configuration
In the sample there is the sbcsettings.json file below

```json
{
  "ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>",
    "senderToQueueConnectionString": "<your_servicebus_connection_string>"
  },
  "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```
**Replace <your_servicebus_connection_string> with queue's connection string.**
**Can you see your queue's connection string following these instructions: [Get the connection string](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-topics-subscriptions-portal#get-the-connection-string)**

3. Crete Azure Blob Storage 
To create the Azure blob storage follow steps at this link: [Create an Azure Storage account article](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

4. Apply your Blob Storage configuration
**Replace <your_blob_storage_connection_string> with blob storage's connection string.**

**Can you see your queue's connection string following instructions at this link:** [View and copy a connection string](https://docs.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string#view-and-copy-a-connection-string)

5. Create Azure Blob Storage Container
To create a container follow steps at this link: [Create a container](https://docs.microsoft.com/it-it/azure/storage/blobs/storage-quickstart-blobs-portal#create-a-container)

**Replace <your_blob_storage_container_name> with blob storage's container name just created.**

6. Configure Topic name in program.cs
in program.cs file look for <your_topic_name> and replace it with the queue name previously created using these instruction [Create a topic using the Azure portal](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-topics-subscriptions-portal#create-a-topic-using-the-azure-portal)

```C#
class Program
    {
        const string ServiceBusConnectionStringName = "senderToQueueConnectionString";
        const string QueueName = "<your_topic_name>";
        static QueueConnector queueClient;
```

7. Run the Console Application

### ConsoleTopicReceicerAppSBC
This sample demonstrate how read message from Topic. This Sample read messages sent by previous sample.

1. Configure Azure Topic
You must use the Topic of the previous sample.

2. Apply your Topics's configuration
Open the solution ConsoleSenderAppSBC.sln
In the solution there is the sbcsettings.json file like this:

```json
{
  "ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>",
    "receiverFromQueueConnectionString": "<your_servicebus_connection_string>"
  },
  "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```
**Replace <your_servicebus_connection_string> with queue's connection string** (the same of previous sample).

3. Azure Blob Storage 
You must use the blob storage of the previous sample.

4. Apply your Blob Storage configuration
**Replace <your_blob_storage_connection_string> with blob storage's connection string** (the same of previous sample).

5. Apply Blob Storage Container Name
**Replace <your_blob_storage_container_name> with blob storage's container name** (the same of previous sample).

6. Configure program.cs
- In program.cs file look for <your_topic_name> and replace it with the Topic name of the previous sample.
- In program.cs file look for <your_topic_subscription_name> and replace it with the topic subscription name created following previous instructions: [Create subscriptions to the topic](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quickstart-topics-subscriptions-portal#create-subscriptions-to-the-topic).

```C#
 class Program
    {
        const string ServiceBusConnectionName = "receiverFromTopicConnectionString";
        const string TopicName = "<your_topic_name>";
        const string SubscriptionName = "<your_topic_subscription_name>";
        static TopicMessageReader subscriptionClient;
```

7. Run the Console Application

