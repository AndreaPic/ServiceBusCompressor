using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor
{
    /// <summary>
    /// Class for event raised when message is readed form queue or topic
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize with arguments
        /// </summary>
        /// <param name="message">Message received</param>
        /// <param name="receivedMessage">Original service bus message</param>
        public MessageReceivedEventArgs(EventMessage message, Message receivedMessage)
        {
            ReceivedEventMessage = message;
            ReceivedMessage = receivedMessage;
        }

        /// <summary>
        /// Message received
        /// </summary>
        public EventMessage ReceivedEventMessage { get; private set; }
        /// <summary>
        /// Original Service Bus message
        /// </summary>
        public Message ReceivedMessage { get; private set; }
    }
}
