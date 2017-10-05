using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    using RabbitMQ.Client;

    public class MessageProperties
    {
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

        [Fact]
        public void Cloning()
        {
            var properties = new BasicProperties
                                 {
                                     Headers = new Dictionary<String, Object>
                                                   {
                                                       { "a", "b" }
                                                   },
                                     AppId = "some-app-id",
                                     ClusterId = "2",
                                     ContentEncoding = "utf-8",
                                     ContentType = "application/json",
                                     CorrelationId = "3",
                                     DeliveryMode = 1,
                                     Expiration = "22",
                                     MessageId = "message-id",
                                     Priority = 1,
                                     ReplyTo = "",
                                     Timestamp = new AmqpTimestamp(123456789L),
                                     Type = "some-type",
                                     UserId = "user-id"
                                 };
            Assert.Equal(properties,
                         (BasicProperties)((IBasicProperties)properties).Clone(),
                         new BasicPropertiesEqualityComparer());
        }

        internal class BasicPropertiesEqualityComparer : IEqualityComparer<BasicProperties>
        {
            public Boolean Equals(BasicProperties x, BasicProperties y)
            {
                if (x == null || y == null)
                    return false;

                var a = new StringBuilder();
                x.AppendPropertyDebugStringTo(a);
                var b = new StringBuilder();
                y.AppendPropertyDebugStringTo(b);

                return String.Equals(a.ToString(), b.ToString());
            }

            public Int32 GetHashCode(BasicProperties obj)
            {
                var a = new StringBuilder();
                obj.AppendPropertyDebugStringTo(a);

                return a.GetHashCode();
            }
        }
    }
}