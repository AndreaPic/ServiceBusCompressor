using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor.Configuration
{
    public static class SBCSettings
    {
        public static string ServiceBusConnectionString
        {
            get
            {
                return Settings.GetConnectionString(nameof(ServiceBusConnectionString));
            }
        }
        public static string QueueOrTopicName
        {
            get
            {
                return Settings.GetSettingValue(nameof(QueueOrTopicName));
            }
        }
        public static string SubscriptionName
        {
            get
            {
                return Settings.GetSettingValue(nameof(SubscriptionName));
            }
        }
    }
}
