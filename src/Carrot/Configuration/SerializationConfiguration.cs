using System;
using System.Collections.Generic;
using Carrot.Serialization;

namespace Carrot.Configuration
{
    public class SerializationConfiguration
    {
        internal const String DefaultContentType = "application/json";
        internal const String DefaultContentEncoding = "UTF-8";

        private readonly IDictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer> _serializers =
            new Dictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer>
                {
                    { _ => _.MediaType == DefaultContentType, new JsonSerializer() }
                };

        private IContentNegotiator _negotiator = new ContentNegotiator();

        internal SerializationConfiguration() { }

        public void Map(Predicate<ContentNegotiator.MediaTypeHeader> predicate, ISerializer serializer)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            _serializers.Add(predicate, serializer);
        }

        public void NegotiateBy(IContentNegotiator negotiator)
        {
            _negotiator = negotiator ?? throw new ArgumentNullException(nameof(negotiator));
        }

        internal virtual ISerializer Create(String contentType)
        {
            if (contentType == null)
                throw new ArgumentNullException(nameof(contentType));

            var result = _negotiator.Negotiate(contentType);

            foreach (var header in result)
                foreach (var serializer in _serializers)
                    if (serializer.Key(header))
                        return serializer.Value;

            return NullSerializer.Instance;
        }
    }
}