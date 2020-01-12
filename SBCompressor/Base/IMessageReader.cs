using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Interface for topic or queue readers
    /// </summary>
    public interface IMessageReader
    {
        /// <summary>
        /// Subscribe to the queue or topic of the service bus
        /// </summary>
        /// <returns></returns>
        Task EnsureSubscribe(Action<MessageReceivedEventArgs> onMessageReceived);
        /// <summary>
        /// Close the client
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}
