#if NET6_0 || NET7_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif
using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor
{
    /// <summary>
    /// Wrapper for servicebus message
    /// </summary>
    public class MessageWrapper
    {
        /// <summary>
        /// Initialize new instance
        /// </summary>
        public MessageWrapper()
        {
#if NETCOREAPP3_1 || NET5_0
            Messages = new List<Message>();
#endif
#if NET6_0 || NET7_0
            Messages = new List<ServiceBusMessage>();
#endif
        }
        /// <summary>
        /// Messages for chunks 
        /// </summary>
#if NETCOREAPP3_1 || NET5_0
        public List<Message> Messages { get; set; }
#endif
#if NET6_0 || NET7_0
        public List<ServiceBusMessage> Messages { get; set; }
#endif
        /// <summary>
        /// Message for simple,zip or storage
        /// </summary>
#if NETCOREAPP3_1 || NET5_0
        public Message Message { get; set; }
#endif
#if NET6_0 || NET7_0
        public ServiceBusMessage Message { get; set; }
#endif

        /// <summary>
        /// Delivery mode
        /// </summary>
        public MessageModes MessageMode { get; set; }
    }
}
