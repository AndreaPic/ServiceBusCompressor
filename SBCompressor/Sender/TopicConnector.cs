using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Send message to Topic
    /// </summary>
    public class TopicConnector : BaseConnector<TopicClient>
    {
        /// <summary>
        /// Create a message sender to service bus
        /// </summary>
        /// <param name="entityName">Queue Name</param>
        /// <param name="connectionStringName">Connection string name to use to look for the connection string in the settings file</param>
        public TopicConnector(string entityName, string connectionStringName) : base(entityName, connectionStringName){}

        /// <summary>
        /// Create a message sender to service bus
        /// </summary>
        /// <param name="entityName">Queue Name</param>
        /// <param name="connectionStringName">Connection string name to use to look for the connection string in the settings file</param>
        /// <param name="serializer">Object used to serialize message</param>
        public TopicConnector(string entityName, string connectionStringName, IMessageSerializer serializer) : base(entityName, connectionStringName, serializer) { }

        /// <summary>
        /// Create topic inside service bus
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo of the service bus</param>
        /// <returns></returns>
        protected override async Task CreateInsideNamespaceAsync(NamespaceInfo namespaceInfo)
        {
            ManagementClient managementClient = new ManagementClient(ServiceBusConnectionString);
            TopicDescription td = new TopicDescription(EntityName);
            td.DefaultMessageTimeToLive = TimeSpan.Parse("14.00:00:00");//TODO: must be in setting config file
            try
            {
                var qdCreated = await managementClient.CreateTopicAsync(td);
            }
            catch (MessagingEntityAlreadyExistsException)
            {

            }
        }

        /// <summary>
        /// Create new TopicClient to send messge to the queue
        /// </summary>
        /// <param name="namespaceInfo">NamespaceInfo of the queue</param>
        /// <returns>QueueClient to send messge to the queue</returns>
        protected override async Task<TopicClient> CreateClientAsync(NamespaceInfo namespaceInfo)
        {
            TopicClient qClient = new TopicClient(ServiceBusConnectionString, EntityName);
            return await Task.FromResult(qClient);
        }

    }
}
