using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor
{
    public interface IMessageDeserializer
    {
        object DeserializeObjectFromJson(string json);
    }
}
