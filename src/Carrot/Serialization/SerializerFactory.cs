namespace Carrot.Serialization
{
    using System;
    using System.Collections.Generic;

    public class SerializerFactory : ISerializerFactory
    {
        private readonly IDictionary<String, ISerializer> _serializers;

        public SerializerFactory(IDictionary<String, ISerializer> serializers = null)
        {
            this._serializers = serializers ?? new Dictionary<String, ISerializer> { { "application/json", new JsonSerializer() } };
        }

        public ISerializer Create(String contentType)
        {
            return !this._serializers.ContainsKey(contentType)
                       ? NullSerializer.Instance
                       : this._serializers[contentType];
        }
    }
}