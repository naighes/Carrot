using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class MessageProperties
    {
        [Fact]
        public void Copying()
        {
            const String appId = "app-id";
            const String clusterId = "cluster-id";
            const String type = "type";
            const String contentEncoding = "UTF-8";
            const String contentType = "application/xml";
            const String correlationId = "123";
            const Byte deliveryMode = 2;
            const String expiration = "60000";
            const String messageId = "456";
            const Byte priority = 3;
            const String userId = "me";
            const String exchangeType = "direct";
            const String exchangeName = "exchange-name";
            const String routingKey = "routing-key";
            String replyTo = $"{exchangeType}://{exchangeName}/{routingKey}";
            var timestamp = new AmqpTimestamp(1445843868L);

            var properties = new BasicProperties
                                 {
                                     AppId = appId,
                                     ClusterId = clusterId,
                                     Type = type,
                                     ContentEncoding = contentEncoding,
                                     ContentType = contentType,
                                     CorrelationId = correlationId,
                                     DeliveryMode = deliveryMode,
                                     Expiration = expiration,
                                     MessageId = messageId,
                                     Priority = priority,
                                     UserId = userId,
                                     ReplyTo = replyTo,
                                     Headers = new Dictionary<String, Object>
                                                   {
                                                       { "h", "a" }
                                                   },
                                     Timestamp = timestamp
                                 };
            var copy = properties.Copy();

            Assert.Equal(appId, copy.AppId);
            Assert.Equal(clusterId, copy.ClusterId);
            Assert.Equal(type, copy.Type);
            Assert.Equal(contentEncoding, copy.ContentEncoding);
            Assert.Equal(contentType, copy.ContentType);
            Assert.Equal(correlationId, copy.CorrelationId);
            Assert.Equal(deliveryMode, copy.DeliveryMode);
            Assert.Equal(expiration, copy.Expiration);
            Assert.Equal(messageId, copy.MessageId);
            Assert.Equal(priority, copy.Priority);
            Assert.Equal(userId, copy.UserId);
            Assert.Equal(replyTo, copy.ReplyTo);
            Assert.Equal(exchangeType, copy.ReplyToAddress.ExchangeType);
            Assert.Equal(exchangeName, copy.ReplyToAddress.ExchangeName);
            Assert.Equal(routingKey, copy.ReplyToAddress.RoutingKey);
            Assert.Equal(1, copy.Headers.Count);
            Assert.True(copy.Headers.ContainsKey("h"));
            Assert.Equal("a", copy.Headers["h"]);
            Assert.Equal(timestamp, copy.Timestamp);
        }

        [Fact]
        public void ReadingRabbitContentType()
        {
            const String contentType = "application/custom";
            var properties = new BasicProperties
                                 {
                                     ContentType = contentType
                                 };
            Assert.Equal(contentType, properties.ContentTypeOrDefault());
        }

        [Fact]
        public void HeadersFallbackContentType()
        {
            const String contentType = "application/custom";
            const String contentEncoding = "UTF-8";
            var encoding = Encoding.GetEncoding(contentEncoding);
            var properties = new BasicProperties
                                 {
                                     ContentEncoding = contentEncoding,
                                     Headers = new Dictionary<String, Object>
                                                   {
                                                       { "Content-Type", encoding.GetBytes(contentType) }
                                                   }
                                 };
            Assert.Equal(contentType, properties.ContentTypeOrDefault());
        }

        [Fact]
        public void DefaultContentType()
        {
            var properties = new BasicProperties();
            Assert.Equal(SerializationConfiguration.DefaultContentType,
                         properties.ContentTypeOrDefault());
        }

        [Fact]
        public void ReadingRabbitContentEncoding()
        {
            const String contentEncoding = "UTF-16";
            var properties = new BasicProperties
            {
                ContentEncoding = contentEncoding
            };
            Assert.Equal(contentEncoding, properties.ContentEncodingOrDefault());
        }

        [Fact]
        public void DefaultContentEncoding()
        {
            var properties = new BasicProperties();
            Assert.Equal(SerializationConfiguration.DefaultContentEncoding,
                         properties.ContentEncodingOrDefault());
        }
    }
}