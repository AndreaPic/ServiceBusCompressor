# Samples description
Samples uses the nuget package of the library.

These set of samples demonstrate how to use this library easily as extension of QueueClient,TopicClient and SubscriptionClient.
These samples are the Microsoft samples modified to use this library.

Samples are:
- ConsoleSenderExtensionAppSBC\ConsoleSenderExtensionAppSBC.sln (example that sends messages to Queue)
- ConsoleReceiverExtensionAppSBC\ConsoleReceiverExtensionAppSBC.sln (example that reads messages from Queue)
- ConsoleTopicSenderExtensionAppSBC\ConsoleTopicSenderExtensionAppSBC.sln (example that sends messages for Topic)
- ConsoleTopicReceicerExtensionAppSBC\ConsoleTopicReceicerExtensionAppSBC.sln (example that reads messages from Topic)

I recommend you to use the samples in this order: 
1. ConsoleSenderExtensionAppSBC.sln (example that sends messages to Queue)
2. ConsoleReceiverExtensionAppSBC.sln (example that reads messages from Queue)
3. ConsoleTopicSenderExtensionAppSBC.sln (example that sends messages for Topic)
4. ConsoleTopicReceicerExtensionAppSBC.sln (example that reads messages from Topic)

## Send messages to the queue
This sample send a message to a Queue.
This sample is the same as [Get started with Service Bus queues](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues#send-messages-to-the-queue).
So you can follow the instruction of the above link (but stop at the paragraph Receive messages from the queue)
The source code is the same with a few line updated.

If you follow the original sample mentioned above, at the end follow these steps:

1. Add Package
Add "SBCCompressor" package from nuget.

2. Add using
Add using to SBCCompressor extensions to send messages:

```C#
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using SBCompressor.Extensions.Sender; //<-- ADD THIS USING	
```

3. Use Extension

Remove below lines of code from SendMessagesAsync method

```C#
	var message = new Message(Encoding.UTF8.GetBytes(messageBody));

	// Write the body of the message to the console
	Console.WriteLine($"Sending message: {messageBody}");

	// Send the message to the queue
	await queueClient.SendAsync(message);
```

Replace the removed lines of code with this line of code:
```C#
	await queueClient.SendCompressorAsync(messageBody);
```

4. Almost done
Well done!
The sample is working.
If you want to send message with size grater than 256Kb you have to add a file to the prject.
Add a .json file tho the project named sbcsettings.json.
**set the property of the file named "Copy to Output Directory" to "Copy if newer"**

The simple content can be:

```json
{
  "VeryLargeMessage": {
    "Strategy": "Chunk"
  }
}
```

But the best functionality is with this content:

```json
{
"ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>"
  },
 "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```

The sample's message doesn't exceed 256Kb so there is no difference from first and second option.
To use the Storage Strategy you have to configure a Blob Storage.
 
5. Crete Azure Blob Storage 
To create the Azure blob storage follow steps at this link: [Create an Azure Storage account article](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

6. Apply your Blob Storage configuration
**Replace <your_blob_storage_connection_string> with blob storage's connection string.**

7. Create Azure Blob Storage Container
To create a container follow steps at this link: [Create a container](https://docs.microsoft.com/it-it/azure/storage/blobs/storage-quickstart-blobs-portal#create-a-container)

8. Apply Blob Storage Container Name
**Replace <your_blob_storage_container_name> with blob storage's container name just created.**

## Read messages from the queue
This sample read messages from the Queue.
This sample is the same as [Receive messages from the queue](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues#receive-messages-from-the-queue).
So you can follow the instruction of the above link (but stop at the paragraph Receive messages from the queue)
The source code is the same with a few line updated.

If you follow the original sample mentioned above, at the end follow these steps:

1. Add Package
Add "SBCCompressor" package from nuget.

2. Add using
Add using to SBCCompressor extensions to send messages:

```C#
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using SBCompressor.Extensions.Reader; //<-- ADD THIS USING	
```

3. Use Extension

Remove all lines of code from RegisterOnMessageHandlerAndReceiveMessages method

```C#
	// Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
	var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
	{
		// Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
		// Set it according to how many messages the application wants to process in parallel.
		MaxConcurrentCalls = 1,

		// Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
		// False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
		AutoComplete = false
	};

	// Register the function that will process messages
	queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
```

Replace the removed lines of code with this line of code:
```C#
	queueClient.SubscribeCompressor(ProcessMessages);
```

4. Add ProcessMessages methods
Add this method

```C#
	private static void ProcessMessages(MessageReceivedEventArgs e)
	{
		Console.WriteLine($"Received message: SequenceNumber:{e.ReceivedMessage.SystemProperties.SequenceNumber} Body:{e.ReceivedEventMessage.Body}");
	}
```

Now you can remove all unused methods.

5. Almost done
Well done!
The sample is working.
If you want to read message with size grater than 256Kb you have to add a file to the prject.
Add a .json file tho the project named sbcsettings.json.
**set the property of the file named "Copy to Output Directory" to "Copy if newer"**

The simple content can be:

```json
{
  "VeryLargeMessage": {
    "Strategy": "Chunk"
  }
}
```

But the best functionality is with this content:

```json
{
"ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>"
  },
 "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```

You must use the same strategy of the previous sample.
So you must fill the file in the same way of the previous sample.

## Send messages to topic
This sample send a message to a topic.
This sample is the same as [Get started with Service Bus topics](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#send-messages-to-the-topic).
So you can follow the instruction of the above link (but stop at the paragraph Receive messages from the subscription)
The source code is the same with a few line updated.

If you follow the original sample mentioned above, at the end follow these steps:

1. Add Package
Add "SBCCompressor" package from nuget.

2. Add using
Add using to SBCCompressor extensions to send messages:

```C#
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using SBCompressor.Extensions.Sender; //<-- ADD THIS USING	
```

3. Use Extension

Remove below lines of code from SendMessagesAsync method

```C#
	var message = new Message(Encoding.UTF8.GetBytes(messageBody));

	// Write the body of the message to the console
	Console.WriteLine($"Sending message: {messageBody}");

	// Send the message to the queue
	await queueClient.SendAsync(message);
```

Replace the removed lines of code with this line of code:
```C#
	await topicClient.SendCompressorAsync(messageBody);
```

4. Almost done
Well done!
The sample is working.
If you want to send message with size grater than 256Kb you have to add a file to the prject.
Add a .json file tho the project named sbcsettings.json.

The simple content can be:

```json
{
  "VeryLargeMessage": {
    "Strategy": "Chunk"
  }
}
```

But the best functionality is with this content:

```json
{
"ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>"
  },
 "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```

The sample's message doesn't exceed 256Kb so there is no difference from first and second option.
To use the Storage Strategy you have to configure a Blob Storage.
 
5. Crete Azure Blob Storage 
To create the Azure blob storage follow steps at this link: [Create an Azure Storage account article](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal).

6. Apply your Blob Storage configuration
**Replace <your_blob_storage_connection_string> with blob storage's connection string.**

7. Create Azure Blob Storage Container
To create a container follow steps at this link: [Create a container](https://docs.microsoft.com/it-it/azure/storage/blobs/storage-quickstart-blobs-portal#create-a-container)

8. Apply Blob Storage Container Name
**Replace <your_blob_storage_container_name> with blob storage's container name just created.**

## Read messages from topic
This sample read messages from the topic.
This sample is the same as [Receive messages from the subscription](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions#receive-messages-from-the-subscription).
The source code is the same with a few line updated.

If you follow the original sample mentioned above, at the end follow these steps:

1. Add Package
Add "SBCCompressor" package from nuget.

2. Add using
Add using to SBCCompressor extensions to send messages:

```C#
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using SBCompressor;	//<-- ADD THIS USING	
    using SBCompressor.Extensions.Reader; //<-- ADD THIS USING	
```

3. Use Extension

Remove all lines of code from RegisterOnMessageHandlerAndReceiveMessages method

```C#
	// Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
	var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
	{
		// Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
		// Set it according to how many messages the application wants to process in parallel.
		MaxConcurrentCalls = 1,

		// Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
		// False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
		AutoComplete = false
	};

	// Register the function that processes messages.
	subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
```

Replace the removed lines of code with this line of code:
```C#
	subscriptionClient.SubscribeCompressor(ProcessMessages);
```

4. Add ProcessMessages methods
Add this method

```C#
	private static void ProcessMessages(MessageReceivedEventArgs e)
	{
		// Process the message.
		Console.WriteLine($"Received message: SequenceNumber:{e.ReceivedMessage.SystemProperties.SequenceNumber} Body:{e.ReceivedEventMessage.Body}");
	}
```

Now you can remove all unused methods.

5. Almost done
Well done!
The sample is working.
If you want to read message with size grater than 256Kb you have to add a file to the prject.
Add a .json file tho the project named sbcsettings.json.

The simple content can be:

```json
{
  "VeryLargeMessage": {
    "Strategy": "Chunk"
  }
}
```

But the best functionality is with this content:

```json
{
"ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>"
  },
 "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}
```

You must use the same strategy of the previous sample.
So you must fill the file in the same way of the previous sample.
