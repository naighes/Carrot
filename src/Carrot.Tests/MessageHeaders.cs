using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class MessageHeaders
    {
        [Fact]
        public void KeyComparison()
        {
            const String key = "foo";
            const String value = "some-value";
            var collection = new OutboundHeaderCollection();
            collection.AddHeader(key, value);
            Assert.Equal(value, collection[key.ToUpperInvariant()]);
        }

        [Fact]
        public void BuildBasicProperties()
        {
            const String contentType = "application/json";
            const String contentEncoding = "UTF-8";
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            var collection = new OutboundHeaderCollection(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase)
                                                              {
                                                                  { "message_id", messageId },
                                                                  { "timestamp", timestamp },
                                                                  { "content_type", contentType },
                                                                  { "content_encoding", contentEncoding }
                                                              });
            const String key = "foo";
            const String value = "bar";
            collection.AddHeader(key, value);
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(EmptyMessageBinding.Instance);
            var properties = collection.BuildBasicProperties<Foo>(null, null, resolver.Object);

            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(new AmqpTimestamp(timestamp), properties.Timestamp);
            Assert.Equal(contentType, properties.ContentType);
            Assert.Equal(contentEncoding, properties.ContentEncoding);
            Assert.Equal(value, collection[key]);
        }

        [Fact]
        public void AddOrRemoveReservedHeaders()
        {
            var collection = new OutboundHeaderCollection();

            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("message_id", "one-id"));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("timestamp", 12345678L));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("content_type", "application/json"));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("content_encoding", "UTF-8"));

            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("message_id"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("timestamp"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("content_type"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("content_encoding"));
        }

        [Fact]
        public void HeaderIndexer()
        {
            var collection = new HeaderCollection();
            Assert.Throws<InvalidOperationException>(() => collection["message_id"]);
            Assert.Throws<InvalidOperationException>(() => collection["timestamp"]);
            Assert.Throws<InvalidOperationException>(() => collection["content_type"]);
            Assert.Throws<InvalidOperationException>(() => collection["content_encoding"]);
        }
    }
}