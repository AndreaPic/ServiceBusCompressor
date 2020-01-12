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

# How to use this library
## Extension 
You can use this library as extension of QueueClient, TopicClient or SubscriptionClient in very easy way using the extension methods:
- SendCompressorAsync (in SBCompressor.Extensions.Sender namespace) instead of SendAsync 
- SubscribeCompressorAsync (in SBCompressor.Extensions.Reader namespace) instead of RegisterMessageHandler
## Library
You can use Library object's as explained in samples: ConsoleSenderAppSBC, ConsoleReceiverAppSBC, ConsoleTopicSenderAppSBC, ConsoleTopicReceicerAppSBC

# Getting Started using the library 
1. Source code
2. Nuget package. You can use this library building it following this document or you can download its package from nuget at this [link](https://www.nuget.org/packages/SPS.SBCompressor/).
Please follow the samples below to learn how to use it.

# Getting Started with source code
This project is a based on .Net Core and has dependencies on Microsoft.Azure.ServiceBus and Microsoft.Azure.Storage.Blob .nuget packages.
Use AzSB solution to build the SBCompressor Library.
In Samples directory there is a readme.md file to explain how to use and configure them.

## Source Code
In this repository there are library, unit tests and saples source code.
In the root directory there is the AzSB.sln file that include library and unit tests.

In the repository there are:
- AzSB.sln (solution with the main library and unit tests)
- Samples\Extension\ConsoleSenderAppSBC\ConsoleSenderAppSBC.sln (Microsoft Queue sample modified to use this library as extension)
- Samples\Extension\ConsoleReceiverAppSBC\ConsoleReceiverAppSBC.sln (Microsoft Queue sample modified to use this library as extension)
- Samples\Extension\ConsoleTopicSenderAppSBC\ConsoleTopicSenderAppSBC.sln (Microsoft Topic sample modified to use this library as extension)
- Samples\Extension\ConsoleTopicReceicerAppSBC\ConsoleTopicReceicerAppSBC.sln (Microsoft Topic sample modified to use this library as extension)
- Samples\FullLibrary\ConsoleSenderAppSBC\ConsoleSenderAppSBC.sln (example that sends messages to Queue)
- Samples\FullLibrary\ConsoleReceiverAppSBC\ConsoleReceiverAppSBC.sln (example that reads messages from Queue)
- Samples\FullLibrary\ConsoleTopicSenderAppSBC\ConsoleTopicSenderAppSBC.sln (example that sends messages for Topic)
- Samples\FullLibrary\ConsoleTopicReceicerAppSBC\ConsoleTopicReceicerAppSBC.sln (example that reads messages from Topic)

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
