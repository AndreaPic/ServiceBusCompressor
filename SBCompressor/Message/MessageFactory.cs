﻿#if NET6_0 || NET7_0 || NET8_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif
using Newtonsoft.Json;
using SBCompressor.Configuration;
using SBCompressor.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Creation\delivery mode
    /// </summary>
    public enum MessageModes
    {
        /// <summary>
        /// standard if message size is lower then 192Kb (256-64)
        /// </summary>
        Simple = 0,
        /// <summary>
        /// zipped if message size after comperssion is lower then 192Kb (256-64)
        /// </summary>
        GZip = 1,
        /// <summary>
        /// splitted in chunk and sent using batch if message size after compression is greather then 192Kb (256-64)
        /// </summary>
        Chunk = 2,
        /// <summary>
        /// chunk using external storage if message size after compression is greather then 192Kb (256-64)
        /// </summary>
        Storage = 3
    }
    /// <summary>
    /// Identify the state of a single chunk of splitted message 
    /// </summary>
    public enum ChunkStates
    {
        /// <summary>
        /// Indentify the first splitted chunk message
        /// </summary>
        Start = 0,
        /// <summary>
        /// Identify a splitted message that isn't the first or the last
        /// </summary>
        Continue = 1,
        /// <summary>
        /// Identify the last splitted message
        /// </summary>
        End = 2
    }

    /// <summary>
    /// Strategies for large message if message size after compression is greather then 192Kb (256-64)
    /// </summary>
    public enum VeryLargeMessageStrategy
    {
        /// <summary>
        /// splitted in chunk and sent using batch if message size after compression is greather then 192Kb (256-64)
        /// </summary>
        Chunk = 1,
        /// <summary>
        /// chunk using external storage if message size after compression is greather then 192Kb (256-64)
        /// </summary>
        Storage = 2
    }

    /// <summary>
    /// Handle Message wrapper creation for service bus 
    /// </summary>
    public class MessageFactory
    {
        /// <summary>
        /// MessageMode property name for message for service bus
        /// </summary>
        public const string MessageModePropertyName = "MessageMode";
        /// <summary>
        /// Chounk gruop property name for message for service bus
        /// </summary>
        public const string MessageChunkGroupIdPropertyName = "ChunkGroupId";
        /// <summary>
        /// Chunk index property name for message for service bus
        /// </summary>
        public const string MessageChunkIndex = "ChunkIndex";
        /// <summary>
        /// Chunk state property name for message for service bus
        /// </summary>
        public const string MessageChunkStatePropertyName = "ChunkState";
        /// <summary>
        /// Max message size for service bus
        /// </summary>
        private const int MaxMessageSize = 192000;

        /// <summary>
        /// Manager fo Blob storage of the message
        /// </summary>
        private MessageStorage storage;
        /// <summary>
        /// Manager fo Blob storage of the message
        /// </summary>
        private MessageStorage Storage 
        { 
            get
            {
                if (storage==null)
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
        /// Default constructor
        /// </summary>
        public MessageFactory()
        {
        }
        /// <summary>
        /// With this constructor you can use explicit settings to handle message hosted in blob storage
        /// </summary>
        /// <param name="settingData">settings for messages hosted in blog storage</param>
        public MessageFactory(StorageSettingData settingData)
        {
            CurrentSettingData = settingData;
        }

        /// <summary>
        /// Current setting for messages hosted in blob storage
        /// </summary>
        private StorageSettingData CurrentSettingData { get; set; }

        /// <summary>
        /// Create a MessageWrapper instance from an EventMessage based on message size and strategy
        /// </summary>
        /// <param name="eventMessage">Message to wrap</param>
        /// <param name="veryLargeMessageStrategy">Stategy for very large message</param>
        /// <returns>
        /// Wrapper for the message
        /// </returns>
        public MessageWrapper CreateMessageFromEvent(EventMessage eventMessage, VeryLargeMessageStrategy veryLargeMessageStrategy)
        {
            MessageWrapper ret = new MessageWrapper();
            string jsonMessageString = JsonConvert.SerializeObject(eventMessage);

            if ((jsonMessageString.Length * sizeof(char)) < MaxMessageSize)
            {
                ConfigureWrapperForSimpleMessage(ret, jsonMessageString);
            }
            else
            {
                byte[] bytes = jsonMessageString.Zip();
                if (bytes.LongLength <= MaxMessageSize)
                {
                    ConfigureWrapperForZippedMessage(ret, bytes);
                }
                else
                {
                    switch (veryLargeMessageStrategy)
                    {
                        case VeryLargeMessageStrategy.Chunk:
                            ConfigureWrapperForChunks(ret, bytes);
                            break;
                        case VeryLargeMessageStrategy.Storage:
                            ConfigureWrapperForStorage(ret, bytes);
                            break;
                        default:
                            break;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// Configure wrapper in simple way, original message is lower then size limit
        /// </summary>
        /// <param name="messageWrapper">wrapper to configure</param>
        /// <param name="jsonMessageString">json content for the message</param>
        private static void ConfigureWrapperForSimpleMessage(MessageWrapper messageWrapper, string jsonMessageString)
        {
            messageWrapper.MessageMode = MessageModes.Simple;
#if NET6_0 || NET7_0 || NET8_0
            ServiceBusMessage brokeredMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessageString));
            brokeredMessage.ApplicationProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif
#if NET5_0 || NETCOREAPP3_1
            Message brokeredMessage = new Message(Encoding.UTF8.GetBytes(jsonMessageString));
            brokeredMessage.UserProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif
            messageWrapper.Message = brokeredMessage;
        }
        /// <summary>
        /// Configure wrapper with zipped message
        /// </summary>
        /// <param name="messageWrapper">wrapper to configure</param>
        /// <param name="bytes">Configure wrapper with zipped bytes</param>
        private void ConfigureWrapperForZippedMessage(MessageWrapper messageWrapper, byte[] bytes)
        {
            messageWrapper.MessageMode = MessageModes.GZip;
#if NET6_0 || NET7_0 || NET8_0
            var s = Convert.ToBase64String(bytes);
            ServiceBusMessage brokeredMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(s));
            //ServiceBusMessage brokeredMessage = new ServiceBusMessage(bytes);
            //Message brokeredMessage = new Message(bytes);
            brokeredMessage.ApplicationProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif
#if NET5_0
            var s = Convert.ToBase64String(bytes);
            Message brokeredMessage = new Message(Encoding.UTF8.GetBytes(s));
            //Message brokeredMessage = new Message(bytes);
            brokeredMessage.UserProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif
#if NETCOREAPP3_1
            var s = Convert.ToBase64String(bytes);
            Message brokeredMessage = new Message(Encoding.UTF8.GetBytes(s));
            //Message brokeredMessage = new Message(bytes);
            brokeredMessage.UserProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif

            messageWrapper.Message = brokeredMessage;
        }
        /// <summary>
        /// Configure message for storage (and upload message to storage)
        /// </summary>
        /// <param name="messageWrapper">wrapper to configure</param>
        /// <param name="bytes">byte array for the message</param>
        private void ConfigureWrapperForStorage(MessageWrapper messageWrapper, byte[] bytes)
        {
            messageWrapper.MessageMode = MessageModes.Storage;
#if NET6_0 || NET7_0 || NET8_0
            ServiceBusMessage brokeredMessage = new ServiceBusMessage(bytes);
            brokeredMessage.ApplicationProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif
#if NET5_0 || NETCOREAPP3_1
            Message brokeredMessage = new Message(bytes);
            brokeredMessage.UserProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
#endif
            messageWrapper.Message = brokeredMessage;
            Storage.UploadMessage(messageWrapper);
            messageWrapper.Message.Body = null; //TODO: review
        }
        /// <summary>
        /// Configure message wrapper for chunks
        /// </summary>
        /// <param name="messageWrapper">wrapper to configure</param>
        /// <param name="bytes">byte array for chunks</param>
        private void ConfigureWrapperForChunks(MessageWrapper messageWrapper, byte[] bytes)
        {
            messageWrapper.MessageMode = MessageModes.Chunk;
            string chunkGroupid = Guid.NewGuid().ToString("D");
            long reminder = 0;
            var quot = Math.DivRem(bytes.LongLength, MaxMessageSize, out reminder);
            long position = 0;
            int chunkIndex = 0;
            for (long fraction = 0; fraction < quot; fraction++)
            {
                byte[] chunk = new byte[MaxMessageSize];
                Array.Copy(bytes, position, chunk, 0, MaxMessageSize);
#if NET5_0 || NETCOREAPP3_1
                Message brokeredMessage = new Message(chunk);
                brokeredMessage.UserProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
                if (fraction == 0)
                {
                    brokeredMessage.UserProperties.Add(MessageChunkStatePropertyName, ChunkStates.Start.ToString());
                }
                else
                {
                    brokeredMessage.UserProperties.Add(MessageChunkStatePropertyName, ChunkStates.Continue.ToString());
                }
                brokeredMessage.UserProperties.Add(MessageChunkGroupIdPropertyName, chunkGroupid);
                brokeredMessage.UserProperties.Add(MessageChunkIndex, chunkIndex);
#endif
#if NET6_0 || NET7_0 || NET8_0
                ServiceBusMessage brokeredMessage = new ServiceBusMessage(chunk);
                brokeredMessage.ApplicationProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
                if (fraction == 0)
                {
                    brokeredMessage.ApplicationProperties.Add(MessageChunkStatePropertyName, ChunkStates.Start.ToString());
                }
                else
                {
                    brokeredMessage.ApplicationProperties.Add(MessageChunkStatePropertyName, ChunkStates.Continue.ToString());
                }
                brokeredMessage.ApplicationProperties.Add(MessageChunkGroupIdPropertyName, chunkGroupid);
                brokeredMessage.ApplicationProperties.Add(MessageChunkIndex, chunkIndex);
#endif
                messageWrapper.Messages.Add(brokeredMessage);
                chunkIndex++;
                position += MaxMessageSize;
            }
            if (reminder != 0)
            {
                byte[] chunk = new byte[reminder];
                Array.Copy(bytes, quot * MaxMessageSize, chunk, 0, reminder);
#if NET5_0 || NETCOREAPP3_1
                Message brokeredMessage = new Message(chunk);
                brokeredMessage.UserProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
                brokeredMessage.UserProperties.Add(MessageChunkStatePropertyName, ChunkStates.End.ToString());
                brokeredMessage.UserProperties.Add(MessageChunkGroupIdPropertyName, chunkGroupid);
                brokeredMessage.UserProperties.Add(MessageChunkIndex, chunkIndex);
#endif
#if NET6_0 || NET7_0 || NET8_0
                ServiceBusMessage brokeredMessage = new ServiceBusMessage(chunk);
                brokeredMessage.ApplicationProperties.Add(MessageModePropertyName, messageWrapper.MessageMode.ToString());
                brokeredMessage.ApplicationProperties.Add(MessageChunkStatePropertyName, ChunkStates.End.ToString());
                brokeredMessage.ApplicationProperties.Add(MessageChunkGroupIdPropertyName, chunkGroupid);
                brokeredMessage.ApplicationProperties.Add(MessageChunkIndex, chunkIndex);
#endif
                messageWrapper.Messages.Add(brokeredMessage);
            }
            else
            {
#if NET5_0 || NETCOREAPP3_1
                messageWrapper.Messages.Last().UserProperties[MessageChunkStatePropertyName] = ChunkStates.End.ToString();
#endif
#if NET6_0 || NET7_0 || NET8_0
                messageWrapper.Messages.Last().ApplicationProperties[MessageChunkStatePropertyName] = ChunkStates.End.ToString();
#endif
            }
        }
    }
}
