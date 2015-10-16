using System;
using System.Text;

namespace Carrot.Serialization
{
    public class NullSerializer : ISerializer
    {
        public static readonly ISerializer Instance = new NullSerializer();

        private NullSerializer() { }

        public Object Deserialize(Byte[] body, Type type, Encoding encoding = null)
        {
            return null;
        }

        public String Serialize(Object obj)
        {
            return null;
        }
    }
}