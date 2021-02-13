using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor.Configuration
{
    /// <summary>
    /// Setting informations about storage of large messages
    /// </summary>
    public class StorageSettingData
    {
        /// <summary>
        /// Construtor to set storage infomrations
        /// </summary>
        /// <param name="storageContainerName">Storage Container Name</param>
        /// <param name="blobStorageConnectionString">Storage connection string</param>
        /// <param name="strategy">Strategy to use with large messages</param>
        public StorageSettingData (string storageContainerName, 
                            string blobStorageConnectionString,
                            VeryLargeMessageStrategy strategy)
        {
            StorageContainerName = storageContainerName;
            BlobStorageConnectionString = blobStorageConnectionString;
            Strategy = strategy;
        }
        /// <summary>
        /// Storage container name
        /// </summary>
        public string StorageContainerName { get; private set; }
        /// <summary>
        /// Blob storage connection string
        /// </summary>
        public string BlobStorageConnectionString { get; private set; }
        /// <summary>
        /// Strategy to use with large messages
        /// </summary>
        public VeryLargeMessageStrategy Strategy { get; private set; }
    }
}
