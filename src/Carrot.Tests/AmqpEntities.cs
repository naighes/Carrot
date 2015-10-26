using System;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class AmqpEntities
    {
        [Fact]
        public void DeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var e1 = new Exchange("e", "direct");
            e1.Declare(model.Object);
            model.Verify(_ => _.ExchangeDeclare(e1.Name, e1.Type, false));
        }

        [Fact]
        public void DeclarationWithExplicitDurability()
        {
            var model = new Mock<IModel>();
            var e2 = new Exchange("e", "topic", true);
            e2.Declare(model.Object);
            model.Verify(_ => _.ExchangeDeclare(e2.Name, e2.Type, true));
        }

        [Fact]
        public void BuildingDirectExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Direct(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("direct", exchange.Type);
            Assert.Equal(false, exchange.Durable);
        }

        [Fact]
        public void BuildingDurableDirectExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.DurableDirect(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("direct", exchange.Type);
            Assert.Equal(true, exchange.Durable);
        }

        [Fact]
        public void BuildingFanoutExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Fanout(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("fanout", exchange.Type);
            Assert.Equal(false, exchange.Durable);
        }

        [Fact]
        public void BuildingDurableFanoutExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.DurableFanout(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("fanout", exchange.Type);
            Assert.Equal(true, exchange.Durable);
        }

        [Fact]
        public void BuildingTopicExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Topic(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("topic", exchange.Type);
            Assert.Equal(false, exchange.Durable);
        }

        [Fact]
        public void BuildingDurableTopicExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.DurableTopic(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("topic", exchange.Type);
            Assert.Equal(true, exchange.Durable);
        }

        [Fact]
        public void BuildingHeadersExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Headers(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("headers", exchange.Type);
            Assert.Equal(false, exchange.Durable);
        }

        [Fact]
        public void BuildingDurableHeadersExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.DurableHeaders(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("headers", exchange.Type);
            Assert.Equal(true, exchange.Durable);
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