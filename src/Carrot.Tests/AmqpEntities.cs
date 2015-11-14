using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class AmqpEntities
    {
        private readonly Channel _channel;

        public AmqpEntities()
        {
            _channel = new Channel(new ChannelConfiguration());
        }

        [Fact]
        public void ExchangeDeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var e1 = _channel.DeclareDirectExchange("e");
            e1.Declare(model.Object);
            model.Verify(_ => _.ExchangeDeclare(e1.Name,
                                                e1.Type,
                                                false,
                                                false,
                                                It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void ExchangeDeclarationWithExplicitDurability()
        {
            var model = new Mock<IModel>();
            var e2 = _channel.DeclareDurableTopicExchange("e");
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
            var exchange = _channel.DeclareDirectExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("direct", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingFanoutExchange()
        {
            const String name = "one_exchange";
            var exchange = _channel.DeclareFanoutExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("fanout", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingTopicExchange()
        {
            const String name = "one_exchange";
            var exchange = _channel.DeclareTopicExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("topic", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void BuildingHeadersExchange()
        {
            const String name = "one_exchange";
            var exchange = _channel.DeclareHeadersExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("headers", exchange.Type);
            Assert.Equal(false, exchange.IsDurable);
        }

        [Fact]
        public void ExchangeEquality()
        {
            const String name = "one_exchange";
            var a = new Exchange(name, "direct");
            var b = new Exchange(name, "topic");
            Assert.Equal(a, b);
            var c = new Exchange("another_name", "direct");
            Assert.NotEqual(a, c);
        }

        [Fact]
        public void MultipleBinding()
        {
            var exchange = _channel.DeclareDirectExchange("exchange");
            var queue = _channel.DeclareQueue("queue");
            _channel.DeclareExchangeBinding(exchange, queue, "key");
            Assert.Throws<ArgumentException>(() => _channel.DeclareExchangeBinding(exchange, queue, "key"));
        }

        [Fact]
        public void QueueDeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var queue = _channel.DeclareQueue("q");
            queue.Declare(model.Object);
            model.Verify(_ => _.QueueDeclare(queue.Name,
                                             false,
                                             false,
                                             false,
                                             It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void QueueEquality()
        {
            const String name = "one_queue";
            var a = new Queue(name);
            var b = new Queue(name);
            Assert.Equal(a, b);
            var c = new Queue("another_name");
            Assert.NotEqual(a, c);
        }
    }
}