using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;

namespace SBCompressor
{
    /// <summary>
    /// Rapresent a message for service bus
    /// </summary>
    [DataContract]
    public class EventMessage
    {
        /// <summary>
        /// Initialize EventMessage Instance
        /// </summary>
        public EventMessage()
        {
            Header = new Header();
        }
        /// <summary>
        /// Header of the message
        /// </summary>
        [DataMember]
        public Header Header { get; set; }
        /// <summary>
        /// Content of the message
        /// </summary>
        [DataMember]
        public string Body { get; set; }

        /// <summary>
        /// Name of the object in the content
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// Name of the event of the message
        /// </summary>
        public string EventName { get; set; }

    }

    /// <summary>
    /// Header for the EventMessage
    /// </summary>
    [DataContract]
    public class Header
    {
        /// <summary>
        /// Initialize Header
        /// </summary>
        public Header()
        {
            HeaderValues = new Collection<HeaderValue>();
        }
        /// <summary>
        /// Header value collection
        /// </summary>
        [DataMember]
        public Collection<HeaderValue> HeaderValues { get; set; }

    }

    /// <summary>
    /// Header value for Header
    /// </summary>
    [DataContract]
    public class HeaderValue
    {
        /// <summary>
        /// Initialize HeaderValue instance
        /// </summary>
        public HeaderValue()
        {
            Values = new Collection<string>();
        }
        /// <summary>
        /// Key of the HeaderValue
        /// </summary>
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// Values for the HeaderValue
        /// </summary>
        [DataMember]
        public Collection<string> Values { get; set; }
    }
}
