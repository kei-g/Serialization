using System;
using System.Collections.Generic;
using System.Text;

namespace SnowStep.Serialization
{
    public class BinarySerializableAttribute : Attribute
    {
        public int Order { get; set; }

        internal virtual void Visit(Dictionary<string, Encoding> encodings, string memberName)
        {
        }
    }
}
