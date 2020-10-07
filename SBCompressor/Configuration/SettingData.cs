using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor.Configuration
{
    public class StorageSettingData
    {
        public StorageSettingData (string storageContainerName, 
                            string blobStorageConnectionString,
                            VeryLargeMessageStrategy strategy)
        {
            StorageContainerName = storageContainerName;
            BlobStorageConnectionString = blobStorageConnectionString;
            Strategy = strategy;
        }
        public string StorageContainerName { get; private set; }
        public string BlobStorageConnectionString { get; private set; }
        public VeryLargeMessageStrategy Strategy { get; private set; }
    }
}
