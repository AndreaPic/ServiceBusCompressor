using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor
{
    /// <summary>
    /// Extension to serialize and deserialize any serializable objects
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// Serialize a serializable object to a byte array
        /// </summary>
        /// <typeparam name="T">Type to serialize</typeparam>
        /// <param name="objectToWrite">Instance to serialize</param>
        /// <returns>byte array of object serialization</returns>
        public static byte[] Serialize<T>(this T objectToWrite)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);

                return stream.GetBuffer();
            }
        }
        /// <summary>
        /// Deserialize a byte array of serialized object
        /// </summary>
        /// <typeparam name="T">Typo to deserialize</typeparam>
        /// <param name="arr">byte array of serialed object</param>
        /// <returns>Deserialized object</returns>
        private static async Task<T> Deserialize<T>(this byte[] arr)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                await stream.WriteAsync(arr, 0, arr.Length);
                stream.Position = 0;

                return (T)binaryFormatter.Deserialize(stream);
            }
        }

        /// <summary>
        /// Deserialize a byte array of serialized object
        /// </summary>
        /// <param name="arr">byte array of serialed object</param>
        /// <returns>Deserialized object</returns>
        public static async Task<object> DeserializeObject(this byte[] arr)
        {
            object obj = await arr.Deserialize<object>();
            return obj;
        }
    }
}
