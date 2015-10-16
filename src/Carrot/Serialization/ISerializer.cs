namespace Carrot.Serialization
{
    using System;
    using System.Text;

    public interface ISerializer
    {
        Object Deserialize(Byte[] body, Type type, Encoding encoding = null);

        String Serialize(Object obj);
    }
}