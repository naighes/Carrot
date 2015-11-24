using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Configuration;
using Carrot.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Extensions
{
    internal static class BasicPropertiesExtensions
    {
        internal static IBasicProperties Copy(this IBasicProperties source)
        {
            var result = new BasicProperties
                             {
                                 AppId = source.AppId,
                                 ClusterId = source.ClusterId,
                                 ContentEncoding = source.ContentEncoding,
                                 ContentType = source.ContentType,
                                 CorrelationId = source.CorrelationId,
                                 DeliveryMode = source.DeliveryMode,
                                 Expiration = source.Expiration,
                                 MessageId = source.MessageId,
                                 Priority = source.Priority,
                                 Timestamp = source.Timestamp,
                                 Type = source.Type,
                                 UserId = source.UserId,
                                 ReplyTo = source.ReplyTo
                             };

            if (source.ReplyTo != null && source.ReplyToAddress != null)
                result.ReplyToAddress = source.ReplyToAddress;

            result.Headers = new Dictionary<String, Object>();

            if (source.Headers != null)
                foreach (var header in source.Headers)
                    result.Headers.Add(header.Key, header.Value);

            return result;
        }

        internal static ISerializer CreateSerializer(this IBasicProperties source,
                                                     SerializationConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return configuration.Create(source.ContentType);
        }

        internal static Encoding CreateEncoding(this IBasicProperties source)
        {
            return Encoding.GetEncoding(source.ContentEncoding);
        }
    }
}