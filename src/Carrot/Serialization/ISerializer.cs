using System;
using System.Reflection;
using System.Text;

namespace Carrot.Serialization
{
    public interface ISerializer
    {
        Object Deserialize(ReadOnlyMemory<Byte> body, TypeInfo type, Encoding encoding = null);

        String Serialize(Object obj);
    }
}