using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Text;
using SBCompressor.Extensions.Reader;

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

#if NET5_0
        /// <summary>
        /// Initialize with arguments
        /// </summary>
        /// <param name="message">Message received</param>
        /// <param name="receivedMessage">Original service bus message</param>
        /// <param name="functionInputData">InputBinding function arguments</param>
        public MessageReceivedEventArgs(EventMessage message, Message receivedMessage, FunctionInputData functionInputData) 
        : this (message, receivedMessage)
        {
            ReceivedByteArrayMessage = functionInputData?.ByteArrayMessage;
        }
        /// <summary>
        /// Initialize with arguments
        /// </summary>
        /// <param name="message">Message received</param>
        /// <param name="functionInputData">InputBinding function arguments</param>
        public MessageReceivedEventArgs(EventMessage message, FunctionInputData functionInputData) 
        : this (message, (Message)null)
        {
            ReceivedByteArrayMessage = functionInputData?.ByteArrayMessage;
        }
#endif

        /// <summary>
        /// Message received
        /// </summary>
        public EventMessage ReceivedEventMessage { get; private set; }

        /// <summary>
        /// Original Service Bus message
        /// </summary>
        public Message ReceivedMessage { get; private set; }

#if NET5_0
        /// <summary>
        /// Original Service Bus inbutbinding message 
        /// </summary>
        public byte[] ReceivedByteArrayMessage {get; private set;}
#endif
    }
}
