namespace Carrot.Serialization
{
    using System;

    public interface ISerializerFactory
    {
        ISerializer Create(String contentType);
    }
}