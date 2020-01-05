using Microsoft.Azure.ServiceBus;
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
            Messages = new List<Message>();
        }
        /// <summary>
        /// Messages for chunks 
        /// </summary>
        public List<Message> Messages { get; set; }
        /// <summary>
        /// Message for simple,zip or storage
        /// </summary>
        public Message Message { get; set; }

        /// <summary>
        /// Delivery mode
        /// </summary>
        public MessageModes MessageMode { get; set; }
    }
}
