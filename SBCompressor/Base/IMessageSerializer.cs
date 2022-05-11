using System;
using System.Collections.Generic;
using System.Text;

namespace SBCompressor
{
    public interface IMessageSerializer
    {
        string SerializeObjectToJson(object objectToSerialize);
    }
}
