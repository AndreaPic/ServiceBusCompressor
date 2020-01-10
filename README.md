# Azure Service Bus “by design”
For many right reasons message has size limitation. For example, large message has network latency problem with performance impact, large message has impact on network traffic, message receiver could have problem to manage large message, small is faster than larger and so on.

Azure service bus has many reasons to limit message size and the size limit is 256Kb for “Standard tier” and 1MB for (Premium tier). Premium tier has other differences then Standard tier but the price is much higher.

You can read more details about quota limitations at this [link](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-quotas)

With this library you can send and read messages bigger then 1Mb without upgrade service bus's tier.

# Issues with those limitation
## Chatty messages
Frequently, to be sure to respect quota limit, the message contains only the identity of the object for which the event was raised and some information about the event.

In this situation who receive the message hasn’t sufficient information about the object and the only thing that can do is to recall a service that host the object and ask for information about the object using the identity contained in the event.

In this scenario when something happen to one object, this object send a message to service bus, and this message is consumed by event subscribers (subscribers can be many). At this point, every subscriber call the service’s object to ask information about it.

Therefore, for one message sent to service bus there are one message for each subscriber and one message from every subscriber to service‘s object.

It is a good practice to define a general purposed representation of the object represented by the message to reduce the “chatty” problem, for this, the message contains the most important information but not all the information or the complete object. In this way, only subscribers that needs information not contained in the message will recall the service’s object.

## Quota exceeded
Even if you have a message object that usually is smaller than 256 Kb, this object could exceed the size limitation in unexpected manner.

The object can have string properties that together exceed the limit.

The object can have an array of properties or an array of objects and the array has not size limit. In this scenario growing the size of the array, increase the probability to exceed the limit. 

In those scenarios, you cannot be sure that your message will never exceed the size limitation.

## Expensive solution
You can adopt “Premium Tier” so you can exceed the 256Kb, anyway this tier has 1 Mb quota limitation.

“Premium Tier” has much performance than “Standard Tier” so is very expensive. I do not think this is the right solution for the message size problem. Anyway, the message’s size limit remain.

# The proposed solution
The proposed solution is applicable to all tier from “Basic” to “Standard” and “Premium”.

The library act as a wrapper of .net object used to send and receive message with Azure Service Bus. 

The library expose a simplified façade to send and receive messages that exceeded the 256kb (or 1 Mb) size limit.

The library implement many strategies to send and receive large message. The strategy will be dynamically selected and will be used transparently.

## "Simple" Strategy
The message is analyzed and if the size is lower than limitation is sent as is. The receiver accept the message without any rework on it.

![Simple strategy](/Documentation/3asb_simple.png "Simple strategy")

- The sender (at step 1) send the message.
- The library (green block) check the message size and if is lower than 256Kb sent it to Azure Service Bus.
- Any receiver get the message form Azure Service Bus (step 2) using the library.
- The library analyze the message (step 3) and understands that do not need any work on it and passes it to the receiver.

## "Compress" Strategy
The message is analyzed and if the size is greater than limit, it will be compressed. The receiver understands that the message is compressed and decompress it before pass it to the receiver.

![Compress strategy](/Documentation/3asb_compressed.png "Compress strategy")

- The sender (at step 1) send the message.
- The library (red block) check the message’s size and if it is greater than 256 Kb, compress the message. After compression if the size is lower than 256Kb (or 1Mb) send the compressed message to Azure Service Bus.
- Any receiver get the message form Azure Service Bus (step 2) using the library.
- The library analyze the message (step 3) and understands that is compressed so decompress it before passes it to the receiver.

If compression is not enough because the size is always greater than 256Kb after compression the library can use other strategies as “chunks” or “storage”.

### "Split" strategy
When compression is not enough, the library can split the compressed message in many message where every message has size lower then 256Kb. The library send all chunks in batch mode to Azure Service Bus.

![Split strategy](/Documentation/3asb_compressedsplitted.png "Compress strategy")

- The sender (at step 1) send the message.
- The library (red block) check the message size and if it is greater than 256 Kb, compress the message. 
- If the size after compression is greater than 256Kb, the library split the message in many chunk with size lower than 256Kb and send them to Azure Service Bus in batch mode.
- Any receiver get the message form Azure Service Bus (step 2) using the library.
- The library analyze the message (step 3) and understands that is a part of compressed and splitted message. The library recompose message, decompress it and pass it to the receiver.

#### Problem with this strategy
If a problem occurs while receiving a message with a chunk and the problem is not notified to Azure Service Bus, the latter does not retry to resend the message so the entire message will be corrupted and impossible to recover. This is a poor solution but require less Azure configurations.

### "Store" strategy
Storage strategy is a good alternative to "Split" strategy.

The library can be configured to use "Storage" strategy instead of "Split" strategy.
*Storage is the default strategy when the messsage exceed the size limit.*

If compressed message size is greater than 256Kb library can put the message in a blob storage.
At this time, the storage can be only a blob storage (today other ways like DocumentDB or MongoDB are not implemented).

![Storage strategy](/Documentation/3asb_compressedstored.png "Storage strategy")

- The sender (at step 1) send the message.
- The library (red block) check the message size and if it is greater than 256 Kb, compress the message. 
- If the size after compression is greater than 256Kb the library put the message into blob storage and send a “stub” message to Azure Service Bus.
- Any receiver get the message form Azure Service Bus (step 2) using the library.
- The library analyze the message (step 3) and understands that is only a stub with information about original message. Then the library get the message from the storage, decompress it and pass it to the receiver.

# Work in Progress
"Store" implementation is not fully completed because nobody remove the message from the storage after every subscriber has successfully reads the message.

Today a solution could be to configure the lifetime of the blob storage. You can configure a rule that delete blobs at the end of their lifecycles to simulate something like a “time to live” option for the messages. 
At this [link](https://docs.microsoft.com/en-us/azure/storage/blobs/storage-lifecycle-management-concepts?tabs=azure-portal), you can read how to configure the lifetime rules.

This is a good solution but there is no guarantee that every subscriber has read his message before the deletion of the message. 
You can configure the blob’s lifetime with a “time to live” greater than “Message time to live” so you will have a great and cheap solution.


# Getting Started with source code
This project is a based on .Net Core and has dependencies on Microsoft.Azure.ServiceBus and Microsoft.Azure.Storage.Blob .nuget packages.
## Source Code
In this there are library, unit tests and saples source code.
In the root directory there is the AzSB.sln file that include all projects.

In the sulution there are:
- AzSB.sln (solution with the main library and unit tests)
- Samples\ConsoleSenderAppSBC\ConsoleSenderAppSBC.sln (example that sends messages to Queue)
- Samples\ConsoleReceiverAppSBC\ConsoleReceiverAppSBC.sln (example that reads messages from Queue)
- Samples\ConsoleTopicSenderAppSBC\ConsoleTopicSenderAppSBC.sln (example that sends messages for Topic)
- Samples\ConsoleTopicReceicerAppSBC\ConsoleTopicReceicerAppSBC.sln (example that reads messages from Topic)

# Build Library and tests
Use AzSB to build the SBCompressor Assembly.
After building SBCompressor Assembly you can use the samples to know ho to use SBCompressor Library.
I Recommend to use the samples in this order: 
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

## How to configure the unit tests

# Getting Started using the library as is
1. Source code
2. Nuget package. You can use this library building it following this document or you can download its package from nuget at this [link](https://www.nuget.org/packages/SPS.SBCompressor/).
Please follow the samples below to learn how to use it.
