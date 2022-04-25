﻿#if NET6_0
using Azure.Messaging.ServiceBus;
#endif
#if NETCOREAPP3_1 || NET5_0
using Microsoft.Azure.ServiceBus;
#endif
using Newtonsoft.Json;
using SBCompressor.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SBCompressor.Extensions.Reader;
using System.IO;

namespace SBCompressor
{
    /// <summary>
    /// Utility for reading messages
    /// </summary>
    internal class ReaderHandler
    {
        /// <summary>
        /// Get message without extra working
        /// </summary>
        /// <param name="receivedMessage">Message from service bus</param>
        /// <returns>Message from service bus</returns>
#if NETCOREAPP3_1 || NET5_0
        internal static EventMessage GetSimpleMessage(Message receivedMessage, Type typeToDeserialize)
#endif
#if NET6_0
        internal static EventMessage GetSimpleMessage(ServiceBusReceivedMessage receivedMessage, Type typeToDeserialize)
#endif
        {
            var jsonMessageString = Encoding.UTF8.GetString(receivedMessage.Body);
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            GetObjectFromMessage(message, typeToDeserialize);
            return message;
        }
        internal static EventMessage GetSimpleMessage(byte[] messageBody, Type typeToDeserialize)
        {
            var jsonMessageString = System.Text.Encoding.UTF8.GetString(messageBody);
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            GetObjectFromMessage(message, typeToDeserialize);
            return message;
        }


        /// <summary>
        /// Set BodyObject of the message argument using message informations.
        /// </summary>
        /// <param name="message">message to work</param>
        /// <param name="typeToDeserialize">Explicit type where deserialize message</param>
        private static void GetObjectFromMessage(EventMessage message, Type typeToDeserialize)
        {
            if (typeToDeserialize!=null)
            {
                var bodyObject = JsonConvert.DeserializeObject(message.Body, typeToDeserialize);
                message.BodyObject = bodyObject;
            }
            else
            {
                GetObjectFromMessage(message);
            }
        }


        /// <summary>
        /// Set BodyObject of the message argument using message informations.
        /// </summary>
        /// <param name="message">message to work</param>
        private static void GetObjectFromMessage(EventMessage message)
        {
            if (!string.IsNullOrEmpty(message.ObjectName))
            {
                Type bodyObjectType = Type.GetType(message.ObjectName);
                if (bodyObjectType != null)
                {
                    var bodyObject = JsonConvert.DeserializeObject(message.Body, bodyObjectType);
                    message.BodyObject = bodyObject;
                }
            }
        }

        /// <summary>
        /// Get the message from compressed state
        /// </summary>
        /// <param name="receivedMessage">Message from queue or topic</param>
        /// <returns>Uncompressed message</returns>
#if NETCOREAPP3_1
        internal static async Task<EventMessage> GetZippedMessage(Message receivedMessage, Type typeToDeserialize)
        {
            var bytes = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(receivedMessage.Body));
            //var bytes = receivedMessage.Body;
#endif
#if NET5_0
        internal static async Task<EventMessage> GetZippedMessage(Message receivedMessage, Type typeToDeserialize)
        {
            var bytes = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(receivedMessage.Body));
            //var bytes = receivedMessage.Body.ToArray();
#endif
#if NET6_0
        internal static async Task<EventMessage> GetZippedMessage(ServiceBusReceivedMessage receivedMessage, Type typeToDeserialize)
        {
            var bytes = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(receivedMessage.Body));
            //var bytes = receivedMessage.Body.ToArray();
#endif
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            GetObjectFromMessage(message, typeToDeserialize);
            return message;
        }

#if NET5_0 || NET6_0
        /// <summary>
        /// Get the message from compressed state
        /// </summary>
        /// <param name="functionInputData">Function inputbinding data</param>
        /// <returns>Uncompressed message</returns>
        internal static async Task<EventMessage> GetZippedMessage(FunctionInputData functionInputData, Type typeToDeserialize)
        {
            var bytes = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(functionInputData.ByteArrayMessage));
            //var bytes = Convert.FromBase64String(messageBody);
            //var bytes = functionInputData.ByteArrayMessage;
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            GetObjectFromMessage(message, typeToDeserialize);
            return message;
        }
#endif
        /// <summary>
        /// Get the message by chunks
        /// </summary>
        /// <param name="receivedMessage">Message from queue or topic</param>
        /// <param name="chunkDictionary">chunk data</param>
        /// <returns>If chunks are completed return the recomposed completed message</returns>
#if NETCOREAPP3_1 || NET5_0
        internal static async Task<EventMessage> GetChunkedMessage(Message receivedMessage, ConcurrentDictionary<string, List<byte[]>> chunkDictionary, Type typeToDeserialize)
        {
            object chunkGroupIdObject = receivedMessage.UserProperties[MessageFactory.MessageChunkGroupIdPropertyName];
            int? chunkIndex = receivedMessage.UserProperties[MessageFactory.MessageChunkIndex] as int?;
            string chunkGroupIdString = Convert.ToString(chunkGroupIdObject);

            byte[] bytes = receivedMessage.Body;
#endif
#if NET6_0
        internal static async Task<EventMessage> GetChunkedMessage(ServiceBusReceivedMessage receivedMessage, ConcurrentDictionary<string, List<byte[]>> chunkDictionary, Type typeToDeserialize)
        {
            object chunkGroupIdObject = receivedMessage.ApplicationProperties[MessageFactory.MessageChunkGroupIdPropertyName];
            int? chunkIndex = receivedMessage.ApplicationProperties[MessageFactory.MessageChunkIndex] as int?;
            string chunkGroupIdString = Convert.ToString(chunkGroupIdObject);

            byte[] bytes = receivedMessage.Body.ToArray();
#endif

            List<byte[]> newBytes = new List<byte[]>();
            newBytes.Add(bytes);

            chunkDictionary.AddOrUpdate(chunkGroupIdString, newBytes, (key, currentList) =>
            {
                currentList.Insert(chunkIndex.Value, bytes);
                return currentList;
            }
            );

#if NETCOREAPP3_1 || NET5_0
            var chunkState = receivedMessage.UserProperties[MessageFactory.MessageChunkStatePropertyName];
#endif
#if NET6_0
            var chunkState = receivedMessage.ApplicationProperties[MessageFactory.MessageChunkStatePropertyName];
#endif
            string chunkStateString = Convert.ToString(chunkState);
            ChunkStates currentChunkState;
            bool parsedState = Enum.TryParse(chunkStateString, out currentChunkState);

            EventMessage ret = null;
            if (parsedState && currentChunkState == ChunkStates.End)
            {
                byte[] completeMessage = new byte[0];
                List<byte[]> chunks = chunkDictionary[chunkGroupIdString];
                foreach (var chunk in chunks)
                {
                    var tmp = completeMessage.Concat(chunk);
                    completeMessage = tmp.ToArray();
                }

                var jsonMessageString = await completeMessage.Unzip() as string;
                var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
                GetObjectFromMessage(message, typeToDeserialize);
                ret = message;
            }
            return ret;
        }

#if NET5_0 || NET6_0
        /// <summary>
        /// Get the message by chunks
        /// </summary>
        /// <param name="functionInputData">Function inputbinding data</param>
        /// <param name="chunkDictionary">chunk data</param>
        /// <returns>If chunks are completed return the recomposed completed message</returns>
        internal static async Task<EventMessage> GetChunkedMessage(FunctionInputData functionInputData, ConcurrentDictionary<string, List<byte[]>> chunkDictionary, Type typeToDeserialize)
        {
            object chunkGroupIdObject = functionInputData.UserProperties[MessageFactory.MessageChunkGroupIdPropertyName];
            int? chunkIndex = functionInputData.UserProperties[MessageFactory.MessageChunkIndex] as int?;
            string chunkGroupIdString = Convert.ToString(chunkGroupIdObject);

            byte[] bytes = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(functionInputData.ByteArrayMessage));
            List<byte[]> newBytes = new List<byte[]>();
            newBytes.Add(bytes);

            chunkDictionary.AddOrUpdate(chunkGroupIdString, newBytes, (key, currentList) =>
            {
                currentList.Insert(chunkIndex.Value, bytes);
                return currentList;
            }
            );

            var chunkState = functionInputData.UserProperties[MessageFactory.MessageChunkStatePropertyName];
            string chunkStateString = Convert.ToString(chunkState);
            ChunkStates currentChunkState;
            bool parsedState = Enum.TryParse(chunkStateString, out currentChunkState);

            EventMessage ret = null;
            if (parsedState && currentChunkState == ChunkStates.End)
            {
                byte[] completeMessage = new byte[0];
                List<byte[]> chunks = chunkDictionary[chunkGroupIdString];
                foreach (var chunk in chunks)
                {
                    var tmp = completeMessage.Concat(chunk);
                    completeMessage = tmp.ToArray();
                }

                var jsonMessageString = await completeMessage.Unzip() as string;
                var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
                GetObjectFromMessage(message, typeToDeserialize);
                ret = message;
            }
            return ret;
        }
#endif

        /// <summary>
        /// Get message from the storage
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <param name="storage">Object to handle stored data</param>
        /// <returns>Event message</returns>
#if NET5_0 || NETCOREAPP3_1
        internal static async Task<EventMessage> GetStoredMessage(Message receivedMessage, MessageStorage storage, Type typeToDeserialize)
#endif
#if NET6_0
        internal static async Task<EventMessage> GetStoredMessage(ServiceBusReceivedMessage receivedMessage, MessageStorage storage, Type typeToDeserialize)
#endif
        {
            var bytes = storage.DownloadMessage(receivedMessage.MessageId);
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            GetObjectFromMessage(message, typeToDeserialize);
            return message;
        }

#if NET5_0 || NET6_0
        /// <summary>
        /// Get message from the storage
        /// </summary>
        /// <param name="functionInputData">Function inputbinding data</param>
        /// <param name="storage">Object to handle stored data</param>
        /// <returns>Event message</returns>
        internal static async Task<EventMessage> GetStoredMessage(FunctionInputData functionInputData, MessageStorage storage, Type typeToDeserialize)
        {
            var bytes = storage.DownloadMessage(functionInputData.MessageId);
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            GetObjectFromMessage(message, typeToDeserialize);
            return message;
        }
#endif

    }
}
