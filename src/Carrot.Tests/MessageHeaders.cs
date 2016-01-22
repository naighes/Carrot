using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Extensions;
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
            var collection = new HeaderCollection();
            collection.AddHeader(key, value);
            Assert.Equal(value, collection[key.ToUpperInvariant()]);
        }

        [Fact]
        public void BuildBasicProperties()
        {
            const String contentType = "application/xml";
            const String contentEncoding = "UTF-16";
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            const String replyTo = "reply-queue-name";
            String correlationId = Guid.NewGuid().ToString();
            var collection = new HeaderCollection(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase)
                                                      {
                                                          { "message_id", messageId },
                                                          { "timestamp", timestamp },
                                                          { "content_type", contentType },
                                                          { "content_encoding", contentEncoding },
                                                          { "correlation_id", correlationId },
                                                          { "reply_to", replyTo }
                                                      });
            const String key = "foo";
            const String value = "bar";
            collection.AddHeader(key, value);
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(EmptyMessageBinding.Instance);
            var message = new OutboundMessage<Foo>(new Foo(), collection);
            var properties = message.BuildBasicProperties(resolver.Object, null, null);

            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(new AmqpTimestamp(timestamp), properties.Timestamp);
            Assert.Equal(contentType, properties.ContentType);
            Assert.Equal(contentEncoding, properties.ContentEncoding);
            Assert.Equal(correlationId, properties.CorrelationId);
            Assert.Equal(replyTo, properties.ReplyTo);
            Assert.Equal(value, collection[key]);
        }

        [Fact]
        public void BuildBasicPropertiesByConfiguration()
        {
            const String contentType = "application/json";
            const String contentEncoding = "UTF-8";
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            var collection = new HeaderCollection(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase)
                                                      {
                                                          { "content_type", contentType },
                                                          { "content_encoding", contentEncoding }
                                                      });
            const String key = "foo";
            const String value = "bar";
            collection.AddHeader(key, value);
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(EmptyMessageBinding.Instance);
            var newId = new Mock<INewId>();
            newId.Setup(_ => _.Next()).Returns(messageId);
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(_ => _.UtcNow()).Returns(timestamp.ToDateTimeOffset());
            var message = new OutboundMessage<Foo>(new Foo(), collection);
            var properties = message.BuildBasicProperties(resolver.Object, dateTimeProvider.Object, newId.Object);

            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(new AmqpTimestamp(timestamp), properties.Timestamp);
            Assert.Equal(contentType, properties.ContentType);
            Assert.Equal(contentEncoding, properties.ContentEncoding);
        }

        [Fact]
        public void AddOrRemoveReservedHeaders()
        {
            var collection = new HeaderCollection();

            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("message_id", "one-id"));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("timestamp", 12345678L));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("content_type", "application/json"));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("content_encoding", "UTF-8"));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("correlation_id", Guid.NewGuid().ToString()));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("reply_to", "reply-queue-name"));

            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("message_id"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("timestamp"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("content_type"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("content_encoding"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("correlation_id"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("reply_to"));
        }

        [Fact]
        public void HeaderIndexer()
        {
            var collection = new HeaderCollection();
            Assert.Throws<InvalidOperationException>(() => collection["message_id"]);
            Assert.Throws<InvalidOperationException>(() => collection["timestamp"]);
            Assert.Throws<InvalidOperationException>(() => collection["content_type"]);
            Assert.Throws<InvalidOperationException>(() => collection["content_encoding"]);
            Assert.Throws<InvalidOperationException>(() => collection["correlation_id"]);
            Assert.Throws<InvalidOperationException>(() => collection["reply_to"]);
        }
    }
}