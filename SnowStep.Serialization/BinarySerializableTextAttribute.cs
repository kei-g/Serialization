using System.Collections.Generic;
using System.Text;

namespace SnowStep.Serialization
{
    public class BinarySerializableTextAttribute : BinarySerializableAttribute
    {
        public string EncodingName { get; set; } = Encoding.UTF8.EncodingName;

        internal override void Visit(Dictionary<string, Encoding> encodings, string memberName)
        {
            encodings.Add(memberName, Encoding.GetEncoding(this.EncodingName));
        }
    }
}
