#if NET6_0 || NET7_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif
using SBCompressor.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SBCompressor.Extensions.Reader
{
    /// <summary>
    /// Base class for message readers
    /// </summary>
    public abstract class MessageReader
    {
        /// <summary>
        /// Default construcotr
        /// </summary>
        public MessageReader()
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
        }
        /// <summary>
        /// Constroctor with setting informations
        /// </summary>
        /// <param name="settingData"></param>
        public MessageReader(StorageSettingData settingData)
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
            CurrentSettingData = settingData;
        }

        /// <summary>
        /// Constroctor with setting informations
        /// </summary>
        /// <param name="settingData"></param>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        public MessageReader(StorageSettingData settingData, Type typeToDeserialize)
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
            CurrentSettingData = settingData;
            TypeToDeserialize = typeToDeserialize;
        }

        /// <summary>
        /// Constroctor with setting informations
        /// </summary>
        /// <param name="settingData"></param>
        /// <param name="messageDeserializer">Object used to deserialize message</param>
        public MessageReader(StorageSettingData settingData, IMessageDeserializer messageDeserializer)
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
            CurrentSettingData = settingData;
            MessageDeserializer = messageDeserializer;
        }

        /// <summary>
        /// Constroctor with setting informations
        /// </summary>
        /// <param name="typeToDeserialize">Type used to deserialize message</param>
        public MessageReader(Type typeToDeserialize)
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
            TypeToDeserialize = typeToDeserialize;
        }

        /// <summary>
        /// Constroctor with setting informations
        /// </summary>
        /// <param name="messageDeserializer">Object used to deserialize message from json</param>
        public MessageReader(IMessageDeserializer messageDeserializer)
        {
            ChunkDictionary = new ConcurrentDictionary<string, List<byte[]>>();
            MessageDeserializer = messageDeserializer;
        }

        private Type TypeToDeserialize { get; set; }

        private IMessageDeserializer MessageDeserializer { get; set; }

        /// <summary>
        /// Storage manager for very large message
        /// </summary>
        private MessageStorage storage;
        /// <summary>
        /// Storage manager for very large message
        /// </summary>
        internal protected MessageStorage Storage
        {
            get
            {
                if (storage == null)
                {
                    if (CurrentSettingData != null)
                    {
                        storage = new MessageStorage(CurrentSettingData);
                    }
                    else
                    {
                        storage = new MessageStorage();
                    }
                }
                return storage;
            }
        }
        /// <summary>
        /// Container for chunks of large messages
        /// </summary>
        private ConcurrentDictionary<string, List<byte[]>> ChunkDictionary { get; set; }

        /// <summary>
        /// Setting informations
        /// </summary>
        private StorageSettingData CurrentSettingData { get; set; }

#if NET5_0 || NETCOREAPP3_1
        /// <summary>
        /// Handler in caso of exceptions reading message
        /// </summary>
        /// <param name="exceptionReceivedEventArgs">information about the exceptions</param>
        /// <returns></returns>
        virtual protected Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            return Task.CompletedTask;
        }
#endif
        /// <summary>
        /// Handle the received message
        /// </summary>
        /// <param name="receivedMessage">message received from service bus</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
#if NET5_0 || NETCOREAPP3_1
        virtual protected async Task MessageReceivedHandler(Message receivedMessage, CancellationToken token)
        {
            try
            {
                // Process message from subscription.
                if (receivedMessage.UserProperties.ContainsKey(MessageFactory.MessageModePropertyName))
                {
                    var prop = receivedMessage.UserProperties[MessageFactory.MessageModePropertyName];
#endif
#if NET6_0 || NET7_0
        virtual protected async Task MessageReceivedHandler(ServiceBusReceivedMessage receivedMessage)//, CancellationToken token)
        {
            try
            {
                // Process message from subscription.
                if (receivedMessage.ApplicationProperties.ContainsKey(MessageFactory.MessageModePropertyName))
                {
                    var prop = receivedMessage.ApplicationProperties[MessageFactory.MessageModePropertyName];
#endif
                    string propName = Convert.ToString(prop);
                    if (!string.IsNullOrEmpty(propName))
                    {
                        MessageModes messageMode;
                        bool parsed = Enum.TryParse(propName, out messageMode);
                        if (parsed)
                        {
                            EventMessage msg = null;
                            switch (messageMode)
                            {
                                case MessageModes.Simple:
                                    msg = ReaderHandler.GetSimpleMessage(receivedMessage, TypeToDeserialize, MessageDeserializer);
                                    await InvokeOnMessageReceived(msg, receivedMessage);
                                    break;
                                case MessageModes.GZip:
                                    msg = await ReaderHandler.GetZippedMessage(receivedMessage, TypeToDeserialize, MessageDeserializer);
                                    await InvokeOnMessageReceived(msg, receivedMessage);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await ReaderHandler.GetChunkedMessage(receivedMessage, ChunkDictionary, TypeToDeserialize, MessageDeserializer);
                                    if (msg != null)
                                    {
                                        await InvokeOnMessageReceived(msg, receivedMessage);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await ReaderHandler.GetStoredMessage(receivedMessage, Storage, TypeToDeserialize, MessageDeserializer);
                                    await InvokeOnMessageReceived(msg, receivedMessage);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }

#if NET6_0 || NET7_0
        virtual protected async Task MessageReceivedHandler(ProcessMessageEventArgs processMessage)//, CancellationToken token)
        {
            await MessageReceivedHandler(processMessage?.Message);
        }
#endif
#if NET5_0 || NET6_0 || NET7_0

        /// <summary>
        /// Handle the received message
        /// </summary>
        /// <param name="functionInputData">Data from the message by inpudbinding arguments</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        virtual protected async Task MessageReceivedHandler(FunctionInputData functionInputData, CancellationToken token)
        {
            try
            {
                // Process message from subscription.
                if (functionInputData.UserProperties.ContainsKey(MessageFactory.MessageModePropertyName))
                {
                    var prop = functionInputData.UserProperties[MessageFactory.MessageModePropertyName];
                    string propName = Convert.ToString(prop);
                    if (!string.IsNullOrEmpty(propName))
                    {
                        MessageModes messageMode;
                        bool parsed = Enum.TryParse(propName, out messageMode);
                        if (parsed)
                        {
                            EventMessage msg = null;
                            switch (messageMode)
                            {
                                case MessageModes.Simple:
                                    msg = ReaderHandler.GetSimpleMessage(functionInputData.ByteArrayMessage, TypeToDeserialize, MessageDeserializer);
                                    await InvokeOnMessageReceived(msg, functionInputData);
                                    break;
                                case MessageModes.GZip:
                                    msg = await ReaderHandler.GetZippedMessage(functionInputData, TypeToDeserialize, MessageDeserializer);
                                    await InvokeOnMessageReceived(msg, functionInputData);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await ReaderHandler.GetChunkedMessage(functionInputData, ChunkDictionary, TypeToDeserialize, MessageDeserializer);
                                    if (msg != null)
                                    {
                                        await InvokeOnMessageReceived(msg, functionInputData);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await ReaderHandler.GetStoredMessage(functionInputData, Storage, TypeToDeserialize, MessageDeserializer);
                                    await InvokeOnMessageReceived(msg, functionInputData);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            catch
            {
                throw;
            }
        }
#endif

        /// <summary>
        /// Action to invoke when a message arrive
        /// </summary>
        protected Action<MessageReceivedEventArgs> OnMessageReceived;

        protected Func<MessageReceivedEventArgs, Task> OnMessageReceivedAsync;


#if NET5_0 || NETCOREAPP3_1
        private async Task InvokeOnMessageReceived(EventMessage message, Message receivedMessage)
#endif
#if NET6_0 || NET7_0
        private async Task InvokeOnMessageReceived(EventMessage message, ServiceBusReceivedMessage receivedMessage)
#endif
        {
            if (OnMessageReceivedAsync!=null)
            {
                await MessageReceivedAsync(message, receivedMessage);
            }
            else
            {
                if (OnMessageReceived != null)
                {
                    MessageReceived(message, receivedMessage);
                }
            }
        }
#if NET5_0 || NET6_0 || NET7_0
        private async Task InvokeOnMessageReceived(EventMessage message, FunctionInputData functionInputData)
        {
            if (OnMessageReceivedAsync != null)
            {
                await MessageReceivedAsync(message, functionInputData);
            }
            else
            {
                if (OnMessageReceived != null)
                {
                    MessageReceived(message, functionInputData);
                }
            }
        }
#endif
        /// <summary>
        /// Raise OnMessageReceived event
        /// </summary>
        /// <param name="message">Message managed by this library</param>
        /// <param name="receivedMessage">Original message from queue or topic</param>
#if NET5_0 || NETCOREAPP3_1
        private void MessageReceived(EventMessage message, Message receivedMessage)
#endif
#if NET6_0 || NET7_0
        private void MessageReceived(EventMessage message, ServiceBusReceivedMessage receivedMessage)
#endif
        {
            OnMessageReceived?.Invoke(new MessageReceivedEventArgs(message, receivedMessage));
        }

#if NET5_0 || NET6_0 || NET7_0
        /// <summary>
        /// Raise OnMessageReceived event
        /// </summary>
        /// <param name="message">Message managed by this library</param>
        /// <param name="functionInputData">Data from the message by inpudbinding arguments</param>
        private void MessageReceived(EventMessage message, FunctionInputData functionInputData)
        {
            OnMessageReceived?.Invoke(new MessageReceivedEventArgs(message, functionInputData));
        }
#endif

        /// <summary>
        /// Raise OnMessageReceived event
        /// </summary>
        /// <param name="message">Message managed by this library</param>
        /// <param name="receivedMessage">Original message from queue or topic</param>
#if NET5_0 || NETCOREAPP3_1
        private async Task MessageReceivedAsync(EventMessage message, Message receivedMessage)
#endif
#if NET6_0 || NET7_0
        private async Task MessageReceivedAsync(EventMessage message, ServiceBusReceivedMessage receivedMessage)
#endif
        {
            await OnMessageReceivedAsync(new MessageReceivedEventArgs(message, receivedMessage));
        }

#if NET5_0 || NET6_0 || NET7_0
        /// <summary>
        /// Raise OnMessageReceived event
        /// </summary>
        /// <param name="message">Message managed by this library</param>
        /// <param name="functionInputData">Data from the message by inpudbinding arguments</param>
        private async Task MessageReceivedAsync(EventMessage message, FunctionInputData functionInputData)
        {
            await OnMessageReceivedAsync(new MessageReceivedEventArgs(message, functionInputData));
        }
#endif

    }
}
