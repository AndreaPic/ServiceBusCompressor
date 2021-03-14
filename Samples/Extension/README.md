# Samples description

Samples uses the nuget package of this library.

These set of samples demonstrate how to use this library easily as extension of QueueClient,TopicClient and SubscriptionClient.
These samples are the Microsoft samples modified to use this library.

Samples are:

- ConsoleSenderExtensionAppSBC\ConsoleSenderExtensionAppSBC.sln (example that sends messages to Queue)
- ConsoleReceiverExtensionAppSBC\ConsoleReceiverExtensionAppSBC.sln (example that reads messages from Queue) **In this solution there are examples that demonstrate how to read message from Azure Functions .net core 3.1 and .net 5. Please read instruction below to execute Azure Functions locally.**
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

The content must be:

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

The content must be:

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

## Read messages from the queue by Azure Functions

in those examples you have to put the queue connection string in the local.settings.json file. Replace `<your_servicebus_connection_string>` with your queue connection string. It's only for this example pourpose, in a real scenario you can put the connection string where you want.

As you can see with this library you can read messages in the same way using .NET 5 or .net core 3.1. Event if the message is an object or a string.

You can read messages from queue or topic in the same way.

### .net core 3.1

To read messages by Azure function in .net core 3.1 set the project "FunctionAppTest31" as StartUp project ("Set as StartUp project" on it).
Execute the project (F5) and "Azure Functions Core Tools" will host locally the Azure Function.

### .NET 5

"SampleApp" is the example that read messages by Azure function in .NET 5.

**YOU CAN NOT DEBUG DIRECTLY USING "Start Debugging" IN VISUAL STUDIO DIRECTLY.** You need to use the command line as mentioned aboce **Run the sample locally** part of this readme.

.NET Isolated in Azure Functions work works differently than a .NET Core 3.1 function app.
You can read about it at official [github repo](https://github.com/Azure/azure-functions-dotnet-worker)

**Run functions locally**

Run `func host start` in the sample app directory.

### Attaching the debugger

#### Visual Studio

> NOTE: To debug your Worker, you must be using the Azure Functions Core Tools version 3.0.3381 or higher

In your worker directory (or your worker's build output directory), run:

```
func host start --dotnet-isolated-debug
```

At this point, your worker process wil be paused, waiting for the debugger to be attached. You can now use Visual Studio to manually attach to the process (to learn more, see [how to attach to a running process](https://docs.microsoft.com/en-us/visualstudio/debugger/attach-to-running-processes-with-the-visual-studio-debugger?view=vs-2019#BKMK_Attach_to_a_running_process))

Once the debugger is attached, the process execution will resume and you will be able to debug.

Details are explained at this [link](https://github.com/Azure/azure-functions-dotnet-worker#attaching-the-debugger).

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

The content must can be:

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

The simple content must be:

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
