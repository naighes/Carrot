using System;
using Xunit;

namespace Carrot.Tests
{
    public class MessageBuilding
    {
        [Fact]
        public void ProperContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, "one-id", 7898L, false, 0L);
            var actual = message.As<Foo>();
            Assert.Equal(content, actual.Content);
        }

        [Fact]
        public void WrongContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, "one-id", 7898L, false, 0L);
            Assert.Throws<InvalidCastException>(() => message.As<Bar>());
        }

        [Fact]
        public void HeaderMapping()
        {
            var content = new Foo();
            const String messageId = "one-id";
            const Int64 timestamp = 123456789L;
            var message = new FakeConsumedMessage(content, messageId, 7898L, false, timestamp);
            var actual = message.As<Foo>();
            Assert.Equal(messageId, actual.Headers.MessageId);
            Assert.Equal(timestamp, actual.Headers.Timestamp);
        }
    }
}