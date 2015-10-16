using System;
using System.Text;

namespace Carrot.Serialization
{
    public interface ISerializer
    {
        Object Deserialize(Byte[] body, Type type, Encoding encoding = null);

        String Serialize(Object obj);
    }
}