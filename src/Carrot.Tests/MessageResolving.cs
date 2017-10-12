using System;
using System.Reflection;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class MessageResolving
    {
        [Fact]
        public void Resolve()
        {
            const String source = "urn:message:foo";
            var args = new BasicDeliverEventArgs { BasicProperties = new BasicProperties { Type = source } };
            var context = ConsumedMessageContext.FromBasicDeliverEventArgs(args);
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.GetTypeInfo().Assembly);
            var binding = resolver.Resolve(context);
            Assert.Equal(source, binding.RawName);
            Assert.Equal(type, binding.RuntimeType);
        }

        [Fact]
        public void CannotResolve()
        {
            const String source = "urn:message:no-resolve";
            var args = new BasicDeliverEventArgs { BasicProperties = new BasicProperties { Type = source } };
            var context = ConsumedMessageContext.FromBasicDeliverEventArgs(args);
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.GetTypeInfo().Assembly);
            Assert.IsType<EmptyMessageBinding>(resolver.Resolve(context));
        }

        [Fact]
        public void ResolveType()
        {
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.GetTypeInfo().Assembly);
            var binding = resolver.Resolve<Foo>();
            Assert.Equal("urn:message:foo", binding.RawName);
            Assert.False(binding.ExpiresAfter.HasValue);
        }

        [Fact]
        public void FallbackResolveType()
        {
            var type = typeof(Bar);
            var resolver = new MessageBindingResolver(type.GetTypeInfo().Assembly);
            var binding = resolver.Resolve<Bar>();
            Assert.Equal("urn:message:Carrot.Tests.Bar", binding.RawName);
        }

        [Fact]
        public void SettingExpiration()
        {
            var type = typeof(Buzz);
            var resolver = new MessageBindingResolver(type.GetTypeInfo().Assembly);
            var binding = resolver.Resolve<Buzz>();
            Assert.Equal("urn:message:buzz", binding.RawName);
            Assert.Equal(TimeSpan.FromSeconds(19), binding.ExpiresAfter);
        }

        [Fact]
        public void Default()
        {
            const String typeName = "Carrot.Tests.Foo";
            var args = new BasicDeliverEventArgs { BasicProperties = new BasicProperties { Type = typeName } };
            var context = ConsumedMessageContext.FromBasicDeliverEventArgs(args);
            var resolver = new DefaultMessageTypeResolver(typeof(Foo).Assembly);
            var binding = resolver.Resolve(context);
            Assert.Equal(typeName, binding.RawName);
            Assert.Equal(typeof(Foo), binding.RuntimeType);

            var binding2 = resolver.Resolve<Foo>();
            Assert.Equal(typeName, binding2.RawName);
            Assert.Equal(typeof(Foo), binding2.RuntimeType);
        }
    }
}