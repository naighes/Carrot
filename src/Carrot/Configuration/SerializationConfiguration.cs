using System;
using System.Collections.Generic;
using Carrot.Serialization;

namespace Carrot.Configuration
{
    public class SerializationConfiguration
    {
        private readonly IDictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer> _serializers =
            new Dictionary<Predicate<ContentNegotiator.MediaTypeHeader>, ISerializer>
                {
                    { _ => _.MediaType == "application/json", new JsonSerializer() }
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
            if (negotiator == null)
                throw new ArgumentNullException(nameof(negotiator));

            _negotiator = negotiator;
        }

        internal virtual ISerializer Create(String contentType)
        {
            var result = _negotiator.Negotiate(contentType);

            foreach (var header in result)
                foreach (var serializer in _serializers)
                    if (serializer.Key(header))
                        return serializer.Value;

            return NullSerializer.Instance;
        }
    }
}