using System;
using System.Collections.Generic;
using System.Globalization;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Extensions
{
    internal static class OutboundMessageExtensions
    {
        private const String DefaultContentEncoding = "UTF-8";
        private const String DefaultContentType = "application/json";

        internal static IBasicProperties BuildBasicProperties<TMessage>(this OutboundMessage<TMessage> message,
                                                                        ChannelConfiguration configuration,
                                                                        IDateTimeProvider dateTimeProvider)
            where TMessage : class
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (dateTimeProvider == null)
                throw new ArgumentNullException(nameof(dateTimeProvider));

            var properties = new BasicProperties
                                 {
                                     Headers = new Dictionary<String, Object>(),
                                     MessageId = configuration.IdGenerator.Next(),
                                     Timestamp = new AmqpTimestamp(dateTimeProvider.UtcNow()
                                                                                   .ToUnixTimestamp())
                                 };
            message.HydrateProperties(properties);
            var binding = configuration.MessageTypeResolver.Resolve<TMessage>();
            properties.Type = binding.RawName;

            if (properties.ContentEncoding == null)
                properties.ContentEncoding = DefaultContentEncoding;

            if (properties.ContentType == null)
                properties.ContentType = DefaultContentType;

            if (binding.ExpiresAfter.HasValue)
                properties.Expiration = binding.ExpiresAfter
                                               .GetValueOrDefault()
                                               .TotalMilliseconds
                                               .ToString(CultureInfo.InvariantCulture);
            return properties;
        }
    }
}