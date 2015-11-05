using System;
using System.Collections.Generic;
using System.Linq;

namespace Carrot.Serialization
{
    public class SerializerFactory : ISerializerFactory
    {
        private readonly IContentNegotiator _negotiator;
        private readonly IDictionary<ContentNegotiator.MediaTypeHeader, ISerializer> _serializers;

        private readonly Dictionary<ContentNegotiator.MediaTypeHeader, ISerializer> _default = new Dictionary<ContentNegotiator.MediaTypeHeader, ISerializer>
                                                   {
                                                       { ContentNegotiator.MediaTypeHeader.Parse("application/json"), new JsonSerializer() }
                                                   };

        public SerializerFactory(IDictionary<String, ISerializer> map = null)
            : this(new ContentNegotiator(), map)
        {
        }

        public SerializerFactory(IContentNegotiator negotiator, IDictionary<String, ISerializer> map = null)
        {
            _negotiator = negotiator ?? new ContentNegotiator();
            _serializers = map != null
                                ? MapSerializers(map)
                                : _default;
        }

        public ISerializer Create(String contentType)
        {
            var result = _negotiator.Negotiate(contentType);

            foreach (var header in result)
                if (_serializers.ContainsKey(header))
                    return _serializers[header];

            return NullSerializer.Instance;
        }

        private static Dictionary<ContentNegotiator.MediaTypeHeader, ISerializer> MapSerializers(IDictionary<String, ISerializer> map)
        {
            return map.Select(ToKeyValuePair).ToDictionary(_ => _.Key, _ => _.Value);
        }

        private static KeyValuePair<ContentNegotiator.MediaTypeHeader, ISerializer> ToKeyValuePair(KeyValuePair<String, ISerializer> pair)
        {
            return new KeyValuePair<ContentNegotiator.MediaTypeHeader, ISerializer>(ContentNegotiator.MediaTypeHeader.Parse(pair.Key),
                                                                                    pair.Value);
        }
    }
}