using System;

namespace Carrot.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Create(String contentType);
    }
}