using System;
using System.Text;

namespace TowerBridge.Common.Infrastructure.Serialization
{
    public interface ISerializer
    {
        Object Deserialize(Byte[] body, Type type, Encoding encoding = null);

        String Serialize(Object obj);
    }
}