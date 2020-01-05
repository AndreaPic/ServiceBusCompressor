using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
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
        public QueueMessageReader(string entityName, string connectionStringName) : base(entityName, connectionStringName){}

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
