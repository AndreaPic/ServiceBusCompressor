using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using SBCompressor.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Common base class for service bus managing
    /// </summary>
    public abstract class ConnectorCore
    {
        /// <summary>
        /// Name of the connection string in the settings file
        /// </summary>
        protected string ConnectionStringName { get; private set; }
        /// <summary>
        /// Queue Name
        /// </summary>
        protected string EntityName { get; private set; }

        /// <summary>
        /// Initialize object to comunicate with service bus
        /// </summary>
        /// <param name="entityName">Queue Name</param>
        /// <param name="connectionStringName">Name of the connection string in the setting file</param>
        public ConnectorCore(string entityName, string connectionStringName)
        {
            EntityName = entityName;
            ConnectionStringName = connectionStringName;
        }

        /// <summary>
        /// Connection string to service bus (readed from the settings file)
        /// </summary>
        protected string ServiceBusConnectionString
        {
            get
            {
                var ret = Settings.CurrentConfiguration.GetConnectionString(ConnectionStringName);
                if (string.IsNullOrEmpty(ret))
                {
                    throw new InvalidConfigurationException();
                }
                return ret;
            }
        }
        /// <summary>
        /// Create NamespaceInfo for the service bus
        /// </summary>
        /// <returns>NamespaceInfo for the service bus</returns>
        virtual protected async Task<NamespaceInfo> CreateNamespaceInfoAsync()
        {
            var connStr = ServiceBusConnectionString;
            ManagementClient managementClient = new ManagementClient(connStr);
            var ret = await managementClient.GetNamespaceInfoAsync();
            return ret;
        }
        /// <summary>
        /// Check if the subscription exists
        /// </summary>
        /// <returns>true if the subscription exists</returns>
        virtual protected async Task<bool> CheckExistSubscription()
        {
            var namespaceInfo = await CreateNamespaceInfoAsync();
            return (namespaceInfo != null); 
        }

    }
}
