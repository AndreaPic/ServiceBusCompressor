using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SBCompressor
{
    /// <summary>
    /// Manage configuration from setting file
    /// </summary>
    internal class Settings
    {
        /// <summary>
        /// Settings file name
        /// </summary>
        private const string AppSettingFileName = "sbcsettings.json";
        /// <summary>
        /// Lazi configuration instance
        /// </summary>
        private static readonly Lazy<IConfiguration> ConfigurationInstance =
        new Lazy<IConfiguration>(
            () =>
            {
                var builder = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(AppSettingFileName, optional: true, reloadOnChange: true);

                IConfiguration configuration = builder.Build();
                return configuration;
            });
        /// <summary>
        /// Current configuration instance
        /// </summary>
        internal static IConfiguration CurrentConfiguration
        {
            get
            {
                var ret = ConfigurationInstance.Value;
                if (ret==null)
                {
                    throw new InvalidConfigurationException();
                }
                return ret;
            }
            
        }
    }
}
