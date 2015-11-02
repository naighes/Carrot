using System;
using Carrot.Configuration;
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
            var binding = resolver.Resolve(source);
            Assert.Equal(source, binding.RawName);
            Assert.Equal(type, binding.RuntimeType);
        }

        [Fact]
        public void CannotResolve()
        {
            const String source = "urn:message:no-resolve";
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.Assembly);
            Assert.IsType<EmptyMessageBinding>(resolver.Resolve(source));
        }

        [Fact]
        public void ResolveType()
        {
            var type = typeof(Foo);
            var resolver = new MessageBindingResolver(type.Assembly);
            var binding = resolver.Resolve<Foo>();
            Assert.Equal("urn:message:foo", binding.RawName);
            Assert.False(binding.ExpiresAfter.HasValue);
        }

        [Fact]
        public void FallbackResolveType()
        {
            var type = typeof(Bar);
            var resolver = new MessageBindingResolver(type.Assembly);
            var binding = resolver.Resolve<Bar>();
            Assert.Equal("urn:message:Carrot.Tests.Bar", binding.RawName);
        }

        [Fact]
        public void SettingExpiration()
        {
            var type = typeof(Buzz);
            var resolver = new MessageBindingResolver(type.Assembly);
            var binding = resolver.Resolve<Buzz>();
            Assert.Equal("urn:message:buzz", binding.RawName);
            Assert.Equal(TimeSpan.FromSeconds(19), binding.ExpiresAfter);
        }
    }
}