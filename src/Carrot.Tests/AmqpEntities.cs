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
        [Fact]
        public void ExchangeDeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var exchange = FakeBroker(model.Object).DeclareDirectExchange("e");
            exchange.Declare(model.Object);
            model.Verify(_ => _.ExchangeDeclare(exchange.Name,
                                                exchange.Type,
                                                false,
                                                false,
                                                It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void ExchangeDeclarationWithExplicitDurability()
        {
            var model = new Mock<IModel>();
            var exchange = FakeBroker(model.Object).DeclareDurableTopicExchange("e");
            exchange.Declare(model.Object);
            model.Verify(_ => _.ExchangeDeclare(exchange.Name,
                                                exchange.Type,
                                                true,
                                                false,
                                                It.IsAny<IDictionary<String, Object>>()));
        }

        [Fact]
        public void BuildingDirectExchange()
        {
            const String name = "one_exchange";
            var exchange = FakeBroker(new Mock<IModel>().Object).DeclareDirectExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("direct", exchange.Type);
            Assert.False(exchange.IsDurable);
        }

        [Fact]
        public void BuildingFanoutExchange()
        {
            const String name = "one_exchange";
            var exchange = FakeBroker(new Mock<IModel>().Object).DeclareFanoutExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("fanout", exchange.Type);
            Assert.False(exchange.IsDurable);
        }

        [Fact]
        public void BuildingTopicExchange()
        {
            const String name = "one_exchange";
            var exchange = FakeBroker(new Mock<IModel>().Object).DeclareTopicExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("topic", exchange.Type);
            Assert.False(exchange.IsDurable);
        }

        [Fact]
        public void BuildingHeadersExchange()
        {
            const String name = "one_exchange";
            var exchange = FakeBroker(new Mock<IModel>().Object).DeclareHeadersExchange(name);
            Assert.Equal(name, exchange.Name);
            Assert.Equal("headers", exchange.Type);
            Assert.False(exchange.IsDurable);
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
            var broker = FakeBroker(new Mock<IModel>().Object);
            var exchange = broker.DeclareDirectExchange("exchange");
            var queue = broker.DeclareQueue("queue");
            broker.DeclareExchangeBinding(exchange, queue, "key");
            Assert.Throws<ArgumentException>(() => broker.DeclareExchangeBinding(exchange,
                                                                                 queue,
                                                                                 "key"));
        }

        [Fact]
        public void QueueDeclarationWithDefaultDurability()
        {
            var model = new Mock<IModel>();
            var queue = FakeBroker(model.Object).DeclareQueue("q");
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

        [Fact]
        public void ExchangeBindingArguments()
        {
            var model = new Mock<IModel>();
            var broker = FakeBroker(model.Object);
            var exchange = broker.DeclareDirectExchange("exchange");
            var queue = broker.DeclareQueue("queue");
            var arguments = new Dictionary<String, Object>
                                {
                                    { "key", "value" }
                                };
            broker.DeclareExchangeBinding(exchange,
                                          queue,
                                          "key",
                                          arguments);
            
            exchange.Declare(model.Object);

            using (broker.Connect())
                model.Verify(_ => _.QueueBind(It.IsAny<String>(),
                                              It.IsAny<String>(),
                                              It.IsAny<String>(),
                                              It.Is<IDictionary<String, Object>>(__ => __ == arguments)));
        }

        [Fact]
        public void QueueArguments()
        {
            var model = new Mock<IModel>();
            var broker = FakeBroker(model.Object);
            var arguments = new Dictionary<String, Object>
                                {
                                    { "key", "value" }
                                };
            broker.DeclareQueue("queue", arguments);

            using (broker.Connect())
                model.Verify(_ => _.QueueDeclare(It.Is<String>(__ => __ == "queue"),
                                                 It.IsAny<Boolean>(),
                                                 It.IsAny<Boolean>(),
                                                 It.IsAny<Boolean>(),
                                                 It.Is<IDictionary<String, Object>>(__ => __ == arguments)));
        }

        [Fact]
        public void ExchangeArguments()
        {
            var model = new Mock<IModel>();
            var broker = FakeBroker(model.Object);
            var arguments = new Dictionary<String, Object>
                                {
                                    { "key", "value" }
                                };
            broker.DeclareDirectExchange("exchange", arguments);

            using (broker.Connect())
                model.Verify(_ => _.ExchangeDeclare(It.Is<String>(__ => __ == "exchange"),
                                                    It.IsAny<String>(),
                                                    It.IsAny<Boolean>(),
                                                    It.IsAny<Boolean>(),
                                                    It.Is<IDictionary<String, Object>>(__ => __ == arguments)));
        }

        private static IBroker FakeBroker(IModel model)
        {
            var builder = new Mock<IConnectionBuilder>();
            var connection = new Mock<RabbitMQ.Client.IConnection>();
            connection.Setup(_ => _.CreateModel()).Returns(model);
            builder.Setup(_ => _.CreateConnection(It.IsAny<Uri>()))
                   .Returns(connection.Object);

            return Broker.New(_ =>
                              {
                                  _.ResolveMessageTypeBy(new Mock<IMessageTypeResolver>().Object);
                              },
                              builder.Object);
        }
    }
}