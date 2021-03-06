﻿using Microsoft.Azure.ServiceBus;
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

        /// <summary>
        /// Handler in caso of exceptions reading message
        /// </summary>
        /// <param name="exceptionReceivedEventArgs">information about the exceptions</param>
        /// <returns></returns>
        virtual protected Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handle the received message
        /// </summary>
        /// <param name="receivedMessage">message received from service bus</param>
        /// <param name="token">CanellationToken</param>
        /// <returns></returns>
        virtual protected async Task MessageReceivedHandler(Message receivedMessage, CancellationToken token)
        {
            try
            {
                // Process message from subscription.
                if (receivedMessage.UserProperties.ContainsKey(MessageFactory.MessageModePropertyName))
                {
                    var prop = receivedMessage.UserProperties[MessageFactory.MessageModePropertyName];
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
                                    msg = ReaderHandler.GetSimpleMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    break;
                                case MessageModes.GZip:
                                    msg = await ReaderHandler.GetZippedMessage(receivedMessage);
                                    MessageReceived(msg, receivedMessage);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await ReaderHandler.GetChunkedMessage(receivedMessage, ChunkDictionary);
                                    if (msg != null)
                                    {
                                        MessageReceived(msg, receivedMessage);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await ReaderHandler.GetStoredMessage(receivedMessage, Storage);
                                    MessageReceived(msg, receivedMessage);
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

#if NET5_0
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
                                    msg = ReaderHandler.GetSimpleMessage(functionInputData.ByteArrayMessage);
                                    MessageReceived(msg, functionInputData);
                                    break;
                                case MessageModes.GZip:
                                    msg = await ReaderHandler.GetZippedMessage(functionInputData);
                                    MessageReceived(msg, functionInputData);
                                    break;
                                case MessageModes.Chunk:
                                    msg = await ReaderHandler.GetChunkedMessage(functionInputData, ChunkDictionary);
                                    if (msg != null)
                                    {
                                        MessageReceived(msg, functionInputData);
                                    }
                                    break;
                                case MessageModes.Storage:
                                    msg = await ReaderHandler.GetStoredMessage(functionInputData, Storage);
                                    MessageReceived(msg, functionInputData);
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


        /// <summary>
        /// Raise OnMessageReceived event
        /// </summary>
        /// <param name="message">Message managed by this library</param>
        /// <param name="receivedMessage">Original message from queue or topic</param>
        private void MessageReceived(EventMessage message, Message receivedMessage)
        {
            OnMessageReceived?.Invoke(new MessageReceivedEventArgs(message, receivedMessage));
        }

#if NET5_0
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

    }
}
