using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SBCompressor.Configuration
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

        /// <summary>
        /// path for settings file to look for strategy
        /// </summary>
        private const string VeryLargeStrategiyettingConfig = "VeryLargeMessage:Strategy";

        /// <summary>
        /// Strategy used for very large message
        /// </summary>
        private static VeryLargeMessageStrategy? strategy;

        /// <summary>
        /// Get the strategy to use for very large message
        /// </summary>
        /// <returns>Current strategy to use for very large message</returns>
        internal static VeryLargeMessageStrategy GetVeryLargeMessageStrategy()
        {
            if (!strategy.HasValue)
            {
                strategy = VeryLargeMessageStrategy.Storage;
                string strategyConfig = Settings.CurrentConfiguration[VeryLargeStrategiyettingConfig];
                if (!string.IsNullOrEmpty(strategyConfig))
                {
                    VeryLargeMessageStrategy tmp;
                    bool parsed = Enum.TryParse(strategyConfig, out tmp);
                    if (parsed)
                    {
                        strategy = tmp;
                    }
                }
            }
            return strategy.Value;
        }

    }
}
