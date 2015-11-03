using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Serialization;
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
            model.Verify(_ => _.ExchangeDeclare(e1.Name,
                                                e1.Type,
                                                false,
                                                false,
                                                It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void DeclarationWithExplicitDurability()
        {
            var model = new Mock<IModel>();
            var e2 = new Exchange("e", "topic", true);
            e2.Declare(model.Object);
            model.Verify(_ => _.ExchangeDeclare(e2.Name,
                                                e2.Type,
                                                true,
                                                false,
                                                It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void BuildingDirectExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Direct(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("direct", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingFanoutExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Fanout(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("fanout", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingTopicExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Topic(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("topic", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingHeadersExchange()
        {
            const String name = "one_exchange";
            var exchange = Exchange.Headers(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("headers", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingDurableExchange()
        {
            var ex = new Exchange("some name", "some type");
            var durableEx = ex.Durable();
            Assert.False(ex.IsDurable);
            Assert.True(durableEx.IsDurable);
            Assert.Equal(ex.Name, durableEx.Name);
            Assert.Equal(ex.Type, durableEx.Type);
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

        [Fact]
        public void MultipleBinding()
        {
            var exchange = Exchange.Direct("exchange");
            var queue = new MessageQueue("queue",
                                         new Mock<IMessageTypeResolver>().Object,
                                         new Mock<ISerializerFactory>().Object);
            exchange.Bind(queue, "key");
            Assert.Throws<ArgumentException>(() => exchange.Bind(queue, "key"));
        }
    }
}