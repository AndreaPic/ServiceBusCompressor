using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Send message to Queue
    /// </summary>
    public class QueueConnector : BaseConnector<QueueClient>
    {
        /// <summary>
        /// Create a message sender to service bus
        /// </summary>
        /// <param name="entityName">Queue Name</param>
        /// <param name="connectionStringName">Connection string name to use to look for the connection string in the settings file</param>
        public QueueConnector(string entityName, string connectionStringName) : base(entityName, connectionStringName){}

        /// <summary>
        /// Create a message sender to service bus
        /// </summary>
        /// <param name="entityName">Queue Name</param>
        /// <param name="connectionStringName">Connection string name to use to look for the connection string in the settings file</param>
        /// <param name="serializer">Object used to serialize message</param>
        public QueueConnector(string entityName, string connectionStringName, IMessageSerializer serializer) : base(entityName, connectionStringName, serializer) { }

        /// <summary>
        /// Create queue inside service bus
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo of the service bus</param>
        /// <returns></returns>
        protected override async Task CreateInsideNamespaceAsync(NamespaceInfo namespaceInfo)
        {
            ManagementClient managementClient = new ManagementClient(ServiceBusConnectionString);
            QueueDescription qd = new QueueDescription(EntityName);
            qd.DefaultMessageTimeToLive = TimeSpan.Parse("14.00:00:00"); //TODO: must be in setting config file
            qd.EnableDeadLetteringOnMessageExpiration = false; //TODO: must be in setting config file
            qd.MaxDeliveryCount = int.MaxValue; //TODO: must be in setting config file
            try
            {
                var qdCreated = await managementClient.CreateQueueAsync(qd);
            }
            catch (MessagingEntityAlreadyExistsException)
            {

            }
        }

        //Do nothing because already configured by someoneelse
        /*
        ///// <summary>
        ///// Configure servicebus inside namespace
        ///// </summary>
        ///// <param name="namespaceInfo">NamespaceInfo of the service bus</param>
        ///// <returns></returns>
        //protected override async Task ConfigureInsideNamespace(string entityName, NamespaceInfo namespaceInfo)
        //{
        //    ManagementClient managementClient = new ManagementClient(NamespaceConnectionString);
        //    QueueDescription qd = new QueueDescription(entityName);
        //    if (qd.MaxDeliveryCount != int.MaxValue)
        //    {
        //        qd.DefaultMessageTimeToLive = TimeSpan.Parse("14.00:00:00");// TimeSpan.MaxValue;
        //        qd.EnableDeadLetteringOnMessageExpiration = false;
        //        qd.MaxDeliveryCount = int.MaxValue;
        //        await managementClient.UpdateQueueAsync(qd);
        //    }
        //}
        */

        /// <summary>
        /// Create new QueueClient to send messge to the queue
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo of the queue</param>
        /// <returns>QueueClient to send messge to the queue</returns>
        protected override async Task<QueueClient> CreateClientAsync(NamespaceInfo namespaceInfo)
        {
            QueueClient qClient = new QueueClient(ServiceBusConnectionString, EntityName, ReceiveMode.PeekLock);
            return await Task.FromResult(qClient);
        }

    }
}
