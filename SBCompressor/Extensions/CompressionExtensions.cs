using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace SBCompressor.Extensions
{
    /// <summary>
    /// Extension class for zip and unzip objects
    /// </summary>
    public static class GZipCompressionExtensions
    {
        /// <summary>
        /// Zip any serializable object
        /// </summary>
        /// <param name="obj">extended object</param>
        /// <returns>byte array of gzipped object</returns>
        public static byte[] Zip(this object obj)
        {
            byte[] bytes = obj.Serialize();

            using (MemoryStream msi = new MemoryStream(bytes))
            {
                using (MemoryStream mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress))
                    {
                        msi.CopyTo(gs);
                    }

                    return mso.ToArray();
                }
            }
        }

        /// <summary>
        /// UnZip a byte array of a zipped object and deserialize the object
        /// </summary>
        /// <param name="bytes">byte array of zipped object</param>
        /// <returns>Uncompressed and deserialized object</returns>
        public static async Task<object> Unzip(this byte[] bytes)
        {
            using (MemoryStream msi = new MemoryStream(bytes))
            {
                using (MemoryStream mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        gs.CopyTo(mso);
                    }

                    return await mso.ToArray().DeserializeObject();
                }
            }
        }
    }
}
