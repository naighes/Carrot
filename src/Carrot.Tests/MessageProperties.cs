using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    using Carrot.Extensions;

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
            const String replyTo = "amqp://my.host";
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
            Assert.Equal(PublicationAddress.Parse(replyTo), copy.ReplyToAddress);
            Assert.Equal(1, copy.Headers.Count);
            Assert.True(copy.Headers.ContainsKey("h"));
            Assert.Equal("a", copy.Headers["h"]);
            Assert.Equal(timestamp, copy.Timestamp);
        }
    }
}