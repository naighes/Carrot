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
            var message = new FakeConsumedMessage(content, "one-id", 7898L, false);
            var actual = message.As<Foo>();
            Assert.Equal(content, actual.Content);
        }

        [Fact]
        public void WrongContentType()
        {
            var content = new Foo();
            var message = new FakeConsumedMessage(content, "one-id", 7898L, false);
            Assert.Throws<InvalidCastException>(() => message.As<Bar>());
        }
    }
}