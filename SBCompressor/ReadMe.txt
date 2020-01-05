Thank you for using this library.
This library need a configuration file named sbcsettings.json.
If you are using visual studio:
1 - add a new file named sbcsettings.json at root path of the project file.
2 - set "Build Action = Nome" and "Copy to Output Directory = Copy if newer"

The content of the file must be:
{
  "ConnectionStrings": {
    "sbcBlobStorageConnectionString": "<your_blob_storage_connection_string>",
    "QueueConnectionString": "<your_servicebus_connection_string>",
    "TopicConnectionString": "<your_servicebus_connection_string>"
  },
  "Storage": {
    "StorageContainerName": "<your_blob_storage_container_name>"
  },
  "VeryLargeMessage": {
    "Strategy": "Storage"
  }
}

You can follow instruction to configure settings file's values at: https://github.com/AndreaPic/ServiceBusCompressor/blob/master/README.md