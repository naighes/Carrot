using System;
using System.Collections.Generic;

namespace TowerBridge.Common.Infrastructure.Serialization
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly IDictionary<String, ISerializer> _serializers;

        public SerializerFactory(IDictionary<String, ISerializer> serializers = null)
        {
            _serializers = serializers ?? new Dictionary<String, ISerializer> { { "application/json", new JsonSerializer() } };
        }

        public ISerializer Create(String contentType)
        {
            return !_serializers.ContainsKey(contentType)
                       ? NullSerializer.Instance
                       : _serializers[contentType];
        }
    }
}