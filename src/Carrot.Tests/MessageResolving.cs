using System;
using Carrot.Messaging;
using Xunit;

namespace Carrot.Tests
{
    public class MessageResolving
    {
        [Fact]
        public void Resolve()
        {
            const String source = "urn:message:foo";
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.Assembly);
            var messageType = resolver.Resolve(source);
            Assert.Equal(source, messageType.RawName);
            Assert.Equal(type, messageType.RuntimeType);
        }

        [Fact]
        public void CannotResolve()
        {
            const String source = "urn:message:no-resolve";
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.Assembly);
            Assert.IsType<EmptyMessageType>(resolver.Resolve(source));
        }

        [Fact]
        public void ResolveType()
        {
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.Assembly);
            var messageType = resolver.Resolve<Foo>();
            Assert.Equal("urn:message:foo", messageType.RawName);
        }

        [Fact]
        public void FallbackResolveType()
        {
            var type = typeof(Bar);
            var resolver = new MessageBindingResolver(type.Assembly);
            var messageType = resolver.Resolve<Bar>();
            Assert.Equal("urn:message:Carrot.Tests.Bar", messageType.RawName);
        }
    }
}