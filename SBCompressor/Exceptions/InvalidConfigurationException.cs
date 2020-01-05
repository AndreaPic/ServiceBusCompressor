using SBCompressor.Resources;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SBCompressor
{
    /// <summary>
    /// Exception for unconfigured settings file
    /// </summary>
    public class InvalidConfigurationException : Exception
    {
        /// <summary>
        /// Initialize Instance
        /// </summary>
        public InvalidConfigurationException() : base() { }
        /// <summary>
        /// Serialization Constructor
        /// </summary>
        /// <param name="info">SerializationInfo</param>
        /// <param name="context">Serialization streaming</param>
        protected InvalidConfigurationException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        { }

        /// <summary>
        /// Message for Invalid Settings File
        /// </summary>
        public override string Message
        {
            get
            {
                return ResourceMessage.InalidSettingsFile;
            }
        }


    }
}
