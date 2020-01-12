using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    public interface IMessageReader
    {
        Task EnsureSubscribe(Action<MessageReceivedEventArgs> onMessageReceived);
        Task CloseAsync();
    }
}
