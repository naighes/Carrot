using System;
using System.Collections.Generic;
using Carrot.Messaging;
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
            var collection = new HeaderCollection();
            collection.AddHeader(key, "some-value");
            Assert.True(collection.Headers.ContainsKey(key.ToUpperInvariant()));
        }

        [Fact]
        public void BuildBasicProperties()
        {
            var collection = new HeaderCollection(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase)
                                                      {
                                                          { "message_id", "one-id" },
                                                          { "timestamp", 123456789L }
                                                      });
            const String key = "foo";
            const String value = "bar";
            collection.AddHeader(key, value);
            var properties = collection.ToOutboundBasicProperties();

            Assert.Null(properties.MessageId);
            Assert.Equal(new AmqpTimestamp(0L), properties.Timestamp);
            Assert.True(collection.Headers.ContainsKey(key));
        }

        [Fact]
        public void AddOrRemoveReservedHeaders()
        {
            var collection = new HeaderCollection();

            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("message_id", "one-id"));
            Assert.Throws<InvalidOperationException>(() => collection.AddHeader("timestamp", 12345678L));

            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("message_id"));
            Assert.Throws<InvalidOperationException>(() => collection.RemoveHeader("timestamp"));
        }
    }
}