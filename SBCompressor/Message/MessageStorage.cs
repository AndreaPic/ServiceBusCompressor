using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
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
        private BlobContainerClient CurrentContainer { get; set; }
        /// <summary>
        /// Initialize new instance
        /// </summary>
        public MessageStorage()
        {
            CurrentContainer = CreateContainerIfNotExist();
        }
        public MessageStorage(StorageSettingData settingData) 
        {
            CurrentSettingData = settingData;
            CurrentContainer = CreateContainerIfNotExist();
        }
        private StorageSettingData CurrentSettingData { get; set; }

        /// <summary>
        /// Get a new storage account using connection string
        /// </summary>
        /// <returns>Storage account using the connection string</returns>
        private BlobContainerClient GetBlobContainerClient()
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
            BlobContainerClient storageAccount = new BlobContainerClient(storageConnectionString,GetContainerName());
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
        private BlobContainerClient CreateContainerIfNotExist()
        {
            var client = GetBlobContainerClient();
            client.CreateIfNotExists();
            return client;
        }

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
                    messageWrapper.Message.MessageId = Guid.NewGuid().ToString();
                    var blob = container.GetBlobClient(messageWrapper.Message.MessageId + ZipExtension);
                    byte[] bodyMessage = messageWrapper.Message.Body;
                    using (MemoryStream stream = new MemoryStream(bodyMessage))
                    {
                        blob.Upload(stream);
                    }
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
            var blob = container.GetBlobClient(messageId + ZipExtension);

            using (MemoryStream ms = new MemoryStream())
            {
                blob.DownloadTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                bodyMessage = ms.ToArray();
            }
            return bodyMessage;
        }
    }
}
