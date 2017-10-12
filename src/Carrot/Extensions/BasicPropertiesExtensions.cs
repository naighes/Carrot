using System;
using System.Linq;
using System.Text;
using Carrot.Configuration;
using Carrot.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Extensions
{
    internal static class BasicPropertiesExtensions
    {
        internal static ISerializer CreateSerializer(this IBasicProperties source,
                                                     SerializationConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            return configuration.Create(source.ContentTypeOrDefault(SerializationConfiguration.DefaultContentType));
        }

        internal static String ContentTypeOrDefault(this IBasicProperties source,
                                                    String @default = "application/json")
        {
            const String key = "Content-Type";

            if (!String.IsNullOrEmpty(source.ContentType))
                return source.ContentType;

            if (source.Headers == null || !source.Headers.ContainsKey(key))
                return @default;

            var bytes = (Byte[])source.Headers[key];

            return bytes.Length > 0 ? source.CreateEncoding().GetString(bytes) : @default;
        }

        internal static Encoding CreateEncoding(this IBasicProperties source)
        {
            return Encoding.GetEncoding(source.ContentEncodingOrDefault(SerializationConfiguration.DefaultContentEncoding));
        }

        internal static String ContentEncodingOrDefault(this IBasicProperties source,
                                                        String @default = "UTF-8")
        {
            return source.ContentEncoding ?? @default;
        }

        internal static IBasicProperties Clone(this IBasicProperties source)
        {
            return new BasicProperties
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
                           ReplyTo = source.ReplyTo,
                           Timestamp = source.Timestamp,
                           Type = source.Type,
                           UserId = source.UserId,
                           Headers = source.Headers.ToDictionary(_ => _.Key, _ => _.Value)
                       };
        }
    }
}