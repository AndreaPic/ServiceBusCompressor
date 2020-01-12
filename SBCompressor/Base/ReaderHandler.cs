using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using SBCompressor.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        internal static EventMessage GetSimpleMessage(Message receivedMessage)
        {
            var jsonMessageString = Encoding.UTF8.GetString(receivedMessage.Body);
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            return message;
        }
        /// <summary>
        /// Get the message from compressed state
        /// </summary>
        /// <param name="receivedMessage">Message from queue or topic</param>
        /// <returns>Uncompressed message</returns>
        internal static async Task<EventMessage> GetZippedMessage(Message receivedMessage)
        {
            var bytes = receivedMessage.Body;
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            return message;
        }

        /// <summary>
        /// Get the message by chunks
        /// </summary>
        /// <param name="receivedMessage">Message from queue or topic</param>
        /// <returns>If chunks are completed return the recomposed completed message</returns>
        internal static async Task<EventMessage> GetChunkedMessage(Message receivedMessage, ConcurrentDictionary<string, List<byte[]>> chunkDictionary)
        {
            object chunkGroupIdObject = receivedMessage.UserProperties[MessageFactory.MessageChunkGroupIdPropertyName];
            int? chunkIndex = receivedMessage.UserProperties[MessageFactory.MessageChunkIndex] as int?;
            string chunkGroupIdString = Convert.ToString(chunkGroupIdObject);

            byte[] bytes = receivedMessage.Body;
            List<byte[]> newBytes = new List<byte[]>();
            newBytes.Add(bytes);

            chunkDictionary.AddOrUpdate(chunkGroupIdString, newBytes, (key, currentList) =>
            {
                currentList.Insert(chunkIndex.Value, bytes);
                return currentList;
            }
            );

            var chunkState = receivedMessage.UserProperties[MessageFactory.MessageChunkStatePropertyName];
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
                ret = message;
            }
            return ret;
        }

        /// <summary>
        /// Get message from the storage
        /// </summary>
        /// <param name="receivedMessage"></param>
        /// <returns>Event message</returns>
        internal static async Task<EventMessage> GetStoredMessage(Message receivedMessage, MessageStorage storage)
        {
            var bytes = storage.DownloadMessage(receivedMessage.MessageId);
            var jsonMessageString = await bytes.Unzip() as string;
            var message = JsonConvert.DeserializeObject<EventMessage>(jsonMessageString);
            return message;
        }


    }
}
