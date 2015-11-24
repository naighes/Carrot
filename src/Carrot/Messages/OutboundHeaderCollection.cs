using System;
using System.Collections.Generic;
using System.Globalization;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Messages
{
    public class OutboundHeaderCollection : HeaderCollection
    {
        private const String DefaultContentEncoding = "UTF-8";
        private const String DefaultContentType = "application/json";

        internal OutboundHeaderCollection() { }

        internal OutboundHeaderCollection(IDictionary<String, Object> dictionary)
            : base(dictionary)
        {
        }

        internal virtual IBasicProperties BuildBasicProperties<TMessage>(INewId idGenerator,
                                                                         IDateTimeProvider dateTimeProvider,
                                                                         IMessageTypeResolver resolver)
            where TMessage : class
        {
            var properties = new BasicProperties
                                 {
                                     Headers = new Dictionary<String, Object>(),
                                     Persistent = false,
                                     ContentType = ContentType ?? DefaultContentType,
                                     ContentEncoding = ContentEncoding ?? DefaultContentEncoding,
                                     MessageId = MessageId ?? idGenerator.Next(),
                                     Timestamp =new AmqpTimestamp(Timestamp <= 0L
                                             ? dateTimeProvider.UtcNow().ToUnixTimestamp()
                                             : Timestamp)
                                 };

            var binding = resolver.Resolve<TMessage>();
            properties.Type = binding.RawName;

            if (binding.ExpiresAfter.HasValue)
                properties.Expiration = binding.ExpiresAfter
                                               .GetValueOrDefault()
                                               .TotalMilliseconds
                                               .ToString(CultureInfo.InvariantCulture);

            foreach (var pair in InternalDictionary)
                if (!ReservedKeys.Contains(pair.Key))
                    properties.Headers.Add(pair.Key, pair.Value);

            return properties;
        }

        public void AddHeader(String key, Object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (ReservedKeys.Contains(key))
                throw new InvalidOperationException($"key '{key}' is reserved");

            InternalDictionary.Add(key, value);
        }

        public void RemoveHeader(String key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (ReservedKeys.Contains(key))
                throw new InvalidOperationException($"key '{key}' is reserved");

            InternalDictionary.Remove(key);
        }
    }
}