using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Extensions
{
    public static class BasicPropertiesExtensions
    {
        public static IBasicProperties Copy(this IBasicProperties source)
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
    }
}