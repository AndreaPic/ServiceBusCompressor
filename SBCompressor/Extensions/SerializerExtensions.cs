using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions
{
    /// <summary>
    /// Extension to serialize and deserialize any serializable objects
    /// </summary>
    public static class SerializerExtensions
    {
        /// <summary>
        /// Get JsonSerializerSettings from JsonConvert.DefaultSettings 
        /// and if it is null create a new instance of JsonSerializerSettings
        /// </summary>
        /// <returns>JsonSerializerSettings</returns>
        private static JsonSerializerSettings GetJSonSerializationSettings()
        {
            JsonSerializerSettings jsonsettings = null;
            if (JsonConvert.DefaultSettings == null)
            {
                jsonsettings = new JsonSerializerSettings()
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                    NullValueHandling = NullValueHandling.Ignore
                };
            }
            else
            {
                jsonsettings = JsonConvert.DefaultSettings();
            }

            return jsonsettings;
        }

        /// <summary>
        /// Serialize a serializable object to a byte array
        /// </summary>
        /// <typeparam name="T">Type to serialize</typeparam>
        /// <param name="objectToWrite">Instance to serialize</param>
        /// <returns>byte array of object serialization</returns>
        /// <remarks>Serialization use JsonConvert.DefaultSettings and UTF8 to convert object in bytearray</remarks>
        public static byte[] Serialize<T>(this T objectToWrite)
        {
            JsonSerializerSettings jsonsettings = GetJSonSerializationSettings();
            var json = JsonConvert.SerializeObject(objectToWrite, jsonsettings);
            return Encoding.UTF8.GetBytes(json);
        }

        /// <summary>
        /// Deserialize a byte array of serialized object
        /// </summary>
        /// <typeparam name="T">Typo to deserialize</typeparam>
        /// <param name="arr">byte array of serialed object</param>
        /// <returns>Deserialized object</returns>
        /// <remarks>bytearray need to be created with UTF8 and JsonConvert.DefaultSettings</remarks>
        private static async Task<T> Deserialize<T>(this byte[] arr)
        {
            var json = Encoding.UTF8.GetString(arr);
            JsonSerializerSettings jsonsettings = GetJSonSerializationSettings();
            T ret = JsonConvert.DeserializeObject<T>(json, jsonsettings);
            return await Task.FromResult(ret);
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
