using System;
using System.Collections.Generic;
using Carrot.Messaging;
using RabbitMQ.Client.Events;
using Xunit;

namespace Carrot.Tests
{
    public class MessageBuilding
    {
        [Fact]
        public void ProperContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, new HeaderCollection(), new BasicDeliverEventArgs());
            var actual = message.As<Foo>();
            Assert.Equal(content, actual.Content);
        }

        [Fact]
        public void WrongContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, new HeaderCollection(), new BasicDeliverEventArgs());
            Assert.Throws<InvalidCastException>(() => message.As<Bar>());
        }

        [Fact]
        public void HeaderMapping()
        {
            var content = new Foo();
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            var headers = new HeaderCollection(new Dictionary<String, Object>
                                                   {
                                                       { "message_id", messageId },
                                                       { "timestamp", timestamp }
                                                   });
            var message = new FakeConsumedMessage(content, headers, new BasicDeliverEventArgs());
            var actual = message.As<Foo>();
            Assert.Equal(messageId, actual.Headers.MessageId);
            Assert.Equal(timestamp, actual.Headers.Timestamp);
        }
    }
}