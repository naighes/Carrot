using System;
using Carrot.Messaging;
using Xunit;

namespace Carrot.Tests
{
    public class AmqpEntities
    {
        [Fact]
        public void BuildingDirectExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Direct(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("direct", exchange.Type);
        }

        [Fact]
        public void BuildingFanoutExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Fanout(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("fanout", exchange.Type);
        }

        [Fact]
        public void BuildingTopicExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Topic(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("topic", exchange.Type);
        }

        [Fact]
        public void BuildingHeadersExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Headers(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("headers", exchange.Type);
        }

        [Fact]
        public void Equality()
        {
            const String name = "one_exchange";
            var a = Exchange.Direct(name);
            var b = Exchange.Topic(name);
            Assert.Equal(a, b);
            var c = Exchange.Direct("another_name");
            Assert.NotEqual(a, c);
        }
    }
}