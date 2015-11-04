using System;
using System.Collections.Generic;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class AmqpEntities
    {
        [Fact]
        public void ExchangeDeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var e1 = Exchange.Direct("e");
            e1.Declare(model.Object, new Mock<IConsumedMessageBuilder>().Object);
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
            var e2 = Exchange.Topic("e").Durable();
            e2.Declare(model.Object, new Mock<IConsumedMessageBuilder>().Object);
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
        public void ExchangeEquality()
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
            var queue = Queue.New("queue");
            exchange.Bind(queue, "key");
            Assert.Throws<ArgumentException>(() => exchange.Bind(queue, "key"));
        }

        [Fact]
        public void QueueDeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var e1 = Queue.New("q");
            e1.Declare(model.Object, new Mock<IConsumedMessageBuilder>().Object);
            model.Verify(_ => _.QueueDeclare(e1.Name,
                                             false,
                                             false,
                                             false,
                                             It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void QueueEquality()
        {
            const String name = "one_queue";
            var a = Queue.New(name);
            var b = Queue.New(name);
            Assert.Equal(a, b);
            var c = Queue.New("another_name");
            Assert.NotEqual(a, c);
        }
    }
}