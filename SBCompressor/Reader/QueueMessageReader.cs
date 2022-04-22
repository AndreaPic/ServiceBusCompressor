using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using SBCompressor.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Queue client for reading operations
    /// </summary>
    public class QueueMessageReader : BaseMessageReader<QueueClient>
    {
        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        public QueueMessageReader(string entityName, string connectionStringName) : base(entityName, connectionStringName) { }
        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="settingData">Explicit Settings</param>
        public QueueMessageReader(string entityName, string connectionStringName, 
            StorageSettingData settingData) 
            : base(entityName, connectionStringName, settingData) 
        { 
        }

        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        public QueueMessageReader(string entityName, string connectionStringName, Type typeToDeserialize) : base(entityName, connectionStringName, typeToDeserialize) { }
        /// <summary>
        /// Initialize new instance
        /// </summary>
        /// <param name="entityName">Queue name</param>
        /// <param name="connectionStringName">Connection string in the settings file</param>
        /// <param name="settingData">Explicit Settings</param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        public QueueMessageReader(string entityName, string connectionStringName,
            StorageSettingData settingData, Type typeToDeserialize)
            : base(entityName, connectionStringName, settingData, typeToDeserialize)
        {
        }

        /// <summary>
        /// Create a new instance of QueueClient
        /// </summary>
        /// <returns>QueueClient used for communicaiton with service bus</returns>
        protected override QueueClient CreateSubscriptionClient()
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, EntityName);
            return queueClient;
        }
    }
}
