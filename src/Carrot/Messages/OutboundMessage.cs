using System;
using System.Collections.Generic;
using System.Globalization;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Messages.Replies;
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
            Headers.Set(HeaderCollection.ContentEncodingKey, value);
        }

        public void SetContentType(String value)
        {
            Headers.Set(HeaderCollection.ContentTypeKey, value);
        }

        public void SetCorrelationId(String correlationId)
        {
            Headers.Set(HeaderCollection.CorrelationIdKey, correlationId);
        }

        public void SetReply(ReplyConfiguration replyConfiguration)
        {
            Headers.Set(HeaderCollection.ReplyConfigurationKey, replyConfiguration);
        }

        internal virtual IBasicProperties BuildBasicProperties(IBasicProperties basicProperties,
                                                               IMessageTypeResolver resolver,
                                                               IDateTimeProvider dateTimeProvider,
                                                               INewId idGenerator)
        {
            basicProperties.Headers = new Dictionary<String, Object>();
            basicProperties.Persistent = false;
            basicProperties.ContentType = Headers.ContentType ?? SerializationConfiguration.DefaultContentType;
            basicProperties.ContentEncoding = Headers.ContentEncoding ?? SerializationConfiguration.DefaultContentEncoding;
            basicProperties.MessageId = Headers.MessageId ?? idGenerator.Next();
            basicProperties.Timestamp = new AmqpTimestamp(Headers.Timestamp <= 0L
                ? dateTimeProvider.UtcNow().ToUnixTimestamp()
                : Headers.Timestamp);
        

            if (!String.IsNullOrWhiteSpace(Headers.CorrelationId))
                basicProperties.CorrelationId = Headers.CorrelationId;

            if (Headers.ReplyConfiguration != null)
                basicProperties.ReplyTo = Headers.ReplyConfiguration.ToString();

            var binding = resolver.Resolve<TMessage>();
            basicProperties.Type = binding.RawName;

            if (binding.ExpiresAfter.HasValue)
                basicProperties.Expiration = binding.ExpiresAfter
                                               .GetValueOrDefault()
                                               .TotalMilliseconds
                                               .ToString(CultureInfo.InvariantCulture);

            Headers.NonReservedHeaders().ForEach(_ => basicProperties.Headers.Add(_.Key, _.Value));

            return basicProperties;
        }
    }
}