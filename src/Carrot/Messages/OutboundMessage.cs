using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Messages
{
    public class OutboundMessage<TMessage> : Message<TMessage>
        where TMessage : class
    {
        public OutboundMessage(TMessage content)
            : this(content, new HeaderCollection())
        {
        }

        internal OutboundMessage(TMessage content, HeaderCollection headers)
        {
            Content = content;
            Headers = headers;
        }

        public override HeaderCollection Headers { get; }

        public override TMessage Content { get; }

        public void AddHeader(String key, Object value)
        {
            Headers.AddHeader(key, value);
        }

        public void RemoveHeader(String key)
        {
            Headers.RemoveHeader(key);
        }

        public void SetContentEncoding(String value)
        {
            Set("content_encoding", value);
        }

        public void SetContentType(String value)
        {
            Set("content_type", value);
        }

        internal virtual IBasicProperties BuildBasicProperties(IMessageTypeResolver resolver,
                                                               IDateTimeProvider dateTimeProvider,
                                                               INewId idGenerator)
        {
            var properties = new BasicProperties
            {
                Headers = new Dictionary<String, Object>(),
                Persistent = false,
                ContentType = Headers.ContentType ?? SerializationConfiguration.DefaultContentType,
                ContentEncoding = Headers.ContentEncoding ?? SerializationConfiguration.DefaultContentEncoding,
                MessageId = Headers.MessageId ?? idGenerator.Next(),
                Timestamp = new AmqpTimestamp(Headers.Timestamp <= 0L
                                             ? dateTimeProvider.UtcNow().ToUnixTimestamp()
                                             : Headers.Timestamp)
            };

            var binding = resolver.Resolve<TMessage>();
            properties.Type = binding.RawName;

            if (binding.ExpiresAfter.HasValue)
                properties.Expiration = binding.ExpiresAfter
                                               .GetValueOrDefault()
                                               .TotalMilliseconds
                                               .ToString(CultureInfo.InvariantCulture);

            Headers.NonReservedHeaders().ToList().ForEach(_ => properties.Headers.Add(_.Key, _.Value));

            return properties;
        }

        private void Set<T>(String key, T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!Headers.InternalDictionary.ContainsKey(key))
                Headers.InternalDictionary.Add(key, value);
            else
                Headers.InternalDictionary[key] = value;
        }
    }
}