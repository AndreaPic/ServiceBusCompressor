using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
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
        private CloudBlobContainer CurrentContainer { get; set; }
        /// <summary>
        /// Initialize new instance
        /// </summary>
        public MessageStorage()
        {
            CurrentContainer = CreateContainerIfNotExist();
        }

        /// <summary>
        /// Get a new storage account using connection string
        /// </summary>
        /// <returns>Storage account using the connection string</returns>
        private CloudStorageAccount GetStorageAccount()
        {
            var storageConnectionString = Settings.CurrentConfiguration.GetConnectionString(BlobConnectionStringConfig);
            if (string.IsNullOrEmpty(storageConnectionString))
            {
                throw new InvalidConfigurationException();
            }
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            return storageAccount;
        }

        /// <summary>
        /// Get a new CloudBlobClient using storage account
        /// </summary>
        /// <returns></returns>
        private CloudBlobClient GetCloudBlobClient()
        {
            var storageAccount = GetStorageAccount();
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            return blobClient;
        }

        /// <summary>
        /// Get the container name using setting file
        /// </summary>
        /// <returns>ContainerName of the blob storage</returns>
        private string GetContainerName()
        {
            string containerName = Settings.CurrentConfiguration[StorageContainerNameConfig];
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
        private CloudBlobContainer CreateContainerIfNotExist()
        {
            var client = GetCloudBlobClient();
            string containerName = GetContainerName();
            var container = client.GetContainerReference(containerName);
            container.CreateIfNotExists();
            return container;
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
                    var blob = container.GetBlockBlobReference(messageWrapper.Message.MessageId + ZipExtension);
                    byte[] bodyMessage = messageWrapper.Message.Body;
                    blob.UploadFromByteArray(bodyMessage, 0, bodyMessage.Length);
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
            using (MemoryStream ms = new MemoryStream())
            {
                var bRef = container.GetBlobReference(messageId + ZipExtension);
                bRef.DownloadToStream(ms);
                ms.Seek(0, SeekOrigin.Begin);
                bodyMessage = ms.ToArray();
            }
            return bodyMessage;
        }
    }
}
