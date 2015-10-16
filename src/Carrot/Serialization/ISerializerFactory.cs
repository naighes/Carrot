using System;

namespace TowerBridge.Common.Infrastructure.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Create(String contentType);
    }
}