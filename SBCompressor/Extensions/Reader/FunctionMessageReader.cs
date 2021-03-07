using Microsoft.Azure.ServiceBus;
using SBCompressor.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Reader
{
    /// <summary>
    /// This class can reveceive message from service bus and take care of very large message and its strategy.
    /// </summary>
    public sealed class FunctionMessageReader : MessageReader
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public FunctionMessageReader()
        {
        }
        /// <summary>
        /// Initialize new instance with explicit setting informations for very large message
        /// </summary>
        /// <param name="settingData">Setting informations for very large message</param>
        public FunctionMessageReader(StorageSettingData settingData) : base(settingData)
        {
        }
        /// <summary>
        /// Subscribe an action for reading message from queue or topic with Azure Function
        /// </summary>
        /// <param name="receivedMessage">Original message triggered by Azure Function</param>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        /// <returns></returns>
        public async Task SubScribe(Message receivedMessage, Action<MessageReceivedEventArgs> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
            CancellationToken token = new CancellationToken();
            await MessageReceivedHandler(receivedMessage, token);
        }
        /// <summary>
        /// Subscribe an action for reading message from queue or topic with Azure Function
        /// </summary>
        /// <param name="receivedMessage">Original message triggered by Azure Function</param>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        public async Task SubScribe(Message receivedMessage, Action<MessageReceivedEventArgs> onMessageReceived, CancellationToken token)
        {
            try
            {
                OnMessageReceived = onMessageReceived;
                await MessageReceivedHandler(receivedMessage, token);
            }
            catch
            {

            }
        }

#if NET5_0
        /// <summary>
        /// Subscribe an action for reading message from queue or topic with Azure Function
        /// </summary>
        /// <param name="functionData">Object hold infomration retrived by input binding of function</param>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        /// <returns></returns>
        public async Task SubScribe(FunctionInputData functionData, Action<MessageReceivedEventArgs> onMessageReceived)
        {
            OnMessageReceived = onMessageReceived;
            CancellationToken token = new CancellationToken();
            await MessageReceivedHandler(functionData, token);
        }
        /// <summary>
        /// Subscribe an action for reading message from queue or topic with Azure Function
        /// </summary>
        /// <param name="functionData">Object hold infomration retrived by input binding of function</param>
        /// <param name="onMessageReceived">Action invoked when a message arrive</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        public async Task SubScribe(FunctionInputData functionData, Action<MessageReceivedEventArgs> onMessageReceived, CancellationToken token)
        {
            try
            {
                OnMessageReceived = onMessageReceived;
                await MessageReceivedHandler(functionData, token);
            }
            catch
            {

            }
        }
#endif
    }

#if NET5_0

    /// <summary>
    /// This DTO contains data from inputbinding of the function
    /// </summary>
    public record FunctionInputData
    {
        /// <summary>
        /// function argument from inputbinding, is "byte[] message"
        /// </summary>
        public byte[] ByteArrayMessage {get; init;}

        /// <summary>
        /// function argument from inputbinding, argiument is "string messageId"
        /// </summary>
        public string MessageId { get; init; }

        /// <summary>
        /// function argument from inputbinding, argiument is "IDictionary<string, object> UserProperties"
        /// </summary>
        public IDictionary<string, object> UserProperties { get; init; }

        /// <summary>
        /// Create new instance using information from function inputbinding
        /// </summary>
        /// <param name="byteArrayMessage">function argument from inputbinding, is "byte[] message"</param>
        /// <param name="messageId">function argument from inputbinding, argiument is "string messageId"</param>
        /// <param name="userProperties">function argument from inputbinding, argiument is "IDictionary<string, object> UserProperties"</param>
        public FunctionInputData(byte[] byteArrayMessage, string messageId, IDictionary<string, object> userProperties)
        {
            ByteArrayMessage = byteArrayMessage;
            MessageId = messageId;
            UserProperties = new Dictionary<string, object>();
            if (userProperties!=null)
            {
                foreach(var item in userProperties)
                {
                    UserProperties.Add(item.Key,item.Value);
                }                 
            }
        }
    }
#endif
}
