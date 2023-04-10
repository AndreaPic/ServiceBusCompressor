using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if NET5_0 || NET6_0 || NET7_0
using Azure.Storage;
using Azure.Storage.Blobs;
#endif
#if NETCOREAPP3_1
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
#endif
using Microsoft.Extensions.Configuration;
using SBCompressor.Configuration;

namespace SBCompressor
{
    /// <summary>
    /// Handle blob storage for messages
    /// </summary>
    public class MessageStorage
    {
        /// <summary>
        /// Extension for the file name in the storage
        /// </summary>
        private const string ZipExtension = ".zip";
        /// <summary>
        /// Connection string name in the setting file for the blobl storage 
        /// </summary>
        private const string BlobConnectionStringConfig = "sbcBlobStorageConnectionString";
        /// <summary>
        /// Path in the setting file for the name of the container in the blob storage
        /// </summary>
        private const string StorageContainerNameConfig = "Storage:StorageContainerName";
        /// <summary>
        /// Container instance
        /// </summary>
#if NET5_0 || NET6_0 || NET7_0
        private BlobContainerClient CurrentContainer { get; set; }
#endif
#if NETCOREAPP3_1
        private CloudBlobContainer CurrentContainer { get; set; }
#endif
        /// <summary>
        /// Initialize new instance
        /// </summary>
        public MessageStorage()
        {
            CurrentContainer = TryCreateContainerIfNotExist();
        }

        /// <summary>
        /// With this constructor you can use explicit settings to handle message hosted in blob storage
        /// </summary>
        /// <param name="settingData">settings for messages hosted in blog storage</param>
        public MessageStorage(StorageSettingData settingData) 
        {
            CurrentSettingData = settingData;
            CurrentContainer = TryCreateContainerIfNotExist();
        }

        /// <summary>
        /// Current setting for messages hosted in blob storage
        /// </summary>
        private StorageSettingData CurrentSettingData { get; set; }

        /// <summary>
        /// Get a new storage account using connection string
        /// </summary>
        /// <returns>Storage account using the connection string</returns>
#if NET5_0 || NET6_0 || NET7_0
        private BlobContainerClient GetBlobContainerClient()
#endif
#if NETCOREAPP3_1
        private CloudStorageAccount GetBlobContainerClient()
#endif
        {
            string storageConnectionString;
            if (CurrentSettingData != null)
            {
                storageConnectionString = CurrentSettingData.BlobStorageConnectionString;
            }
            else
            {
                storageConnectionString = Settings.GetConnectionString(BlobConnectionStringConfig);
            }
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new InvalidConfigurationException();
            }
#if NET5_0 || NET6_0 || NET7_0
            BlobContainerClient storageAccount = new BlobContainerClient(storageConnectionString,GetContainerName());
#endif
#if NETCOREAPP3_1
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
#endif
            return storageAccount;
        }

        /// <summary>
        /// Get the container name using setting file
        /// </summary>
        /// <returns>ContainerName of the blob storage</returns>
        private string GetContainerName()
        {
            string containerName;
            if (CurrentSettingData != null)
            {
                containerName = CurrentSettingData.StorageContainerName;
            }
            else
            {
                containerName = Settings.GetSettingValue(StorageContainerNameConfig);
            }
            if (string.IsNullOrEmpty(containerName))
            {
                throw new InvalidConfigurationException();
            }
            return containerName;
        }

        /// <summary>
        /// Create container if not exists using connection string and container name
        /// </summary>
        /// <returns>CloudBlobContainer</returns>
#if NET5_0 || NET6_0 || NET7_0
        private BlobContainerClient TryCreateContainerIfNotExist()
        {
            var client = GetBlobContainerClient();
            try
            {
                client.CreateIfNotExists();
            }
            catch { } //TODO: manage write permission required
            return client;
        }
#endif
#if NETCOREAPP3_1
        /// <summary>
        /// Get a new CloudBlobClient using storage account
        /// </summary>
        /// <returns></returns>
        private CloudBlobClient GetCloudBlobClient()
        {
            var storageAccount = GetBlobContainerClient();//GetStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }
        /// <summary>
        /// Create container if not exists using connection string and container name
        /// </summary>
        /// <returns>CloudBlobContainer</returns>
        private CloudBlobContainer TryCreateContainerIfNotExist()
        {
            var client = GetCloudBlobClient();
            string containerName = GetContainerName();
            var container = client.GetContainerReference(containerName);
            try
            {
                container.CreateIfNotExists();
            }
            catch { } //TODO: manage write permission required
            return container;
        }
#endif

        /// <summary>
        /// Upload body o messagewrapper to the container
        /// </summary>
        /// <param name="messageWrapper">MessageWrapper to upload</param>
        public void UploadMessage(MessageWrapper messageWrapper)
        {
            switch (messageWrapper.MessageMode)
            {
                case MessageModes.Storage:
                    var container = CurrentContainer;
                    messageWrapper.Message.MessageId = Guid.NewGuid().ToString("N");
#if NETCOREAPP3_1
                    var blob = container.GetBlockBlobReference(messageWrapper.Message.MessageId + ZipExtension);
                    byte[] bodyMessage = messageWrapper.Message.Body;
                    blob.UploadFromByteArray(bodyMessage, 0, bodyMessage.Length);
#endif
#if NET5_0 

                    var blob = container.GetBlobClient(messageWrapper.Message.MessageId + ZipExtension);
                    byte[] bodyMessage = messageWrapper.Message.Body;
                    using (MemoryStream stream = new MemoryStream(bodyMessage))
                    {
                        blob.Upload(stream);
                    }
#endif
#if NET6_0 || NET7_0

                    var blob = container.GetBlobClient(messageWrapper.Message.MessageId + ZipExtension);
                    byte[] bodyMessage = messageWrapper.Message.Body.ToArray();
                    using (MemoryStream stream = new MemoryStream(bodyMessage))
                    {
                        blob.Upload(stream);
                    }
#endif
                    break;
            }
        }

        /// <summary>
        /// Get the body of messagewrapper form the container
        /// </summary>
        /// <param name="messageId">identity of the message</param>
        /// <returns>byte array of the file for the messge</returns>
        public byte[] DownloadMessage(string messageId)
        {
            byte[] bodyMessage = null;
            var container = CurrentContainer;
#if NETCOREAPP3_1
            var bRef = container.GetBlobReference(messageId + ZipExtension);
            using (MemoryStream ms = new MemoryStream())
            {
                bRef.DownloadToStream(ms);
                ms.Seek(0, SeekOrigin.Begin);
                bodyMessage = ms.ToArray();
            }
#endif
#if NET5_0 || NET6_0 || NET7_0
            var blob = container.GetBlobClient(messageId + ZipExtension);
            using (MemoryStream ms = new MemoryStream())
            {
                blob.DownloadTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                bodyMessage = ms.ToArray();
            }
#endif
            return bodyMessage;
        }
    }
}
