using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class Connecting
    {
        [Fact]
        public void InitializationOfAmqpStuff()
        {
            const String queueName = "queue_name";
            const String exchangeName = "exchange_name";
            const String routingKey = "routing_key";

            var model = new Mock<IModel>();
            var configuration = new EnvironmentConfiguration();
            configuration.GeneratesMessageIdBy(new Mock<INewId>().Object);
            configuration.ResolveMessageTypeBy(new Mock<IMessageTypeResolver>().Object);
            var connection = new Mock<RabbitMQ.Client.IConnection>();
            connection.Setup(_ => _.CreateModel()).Returns(model.Object);
            var broker = new BrokerWrapper(connection.Object, model.Object, configuration);
            var consumer = new FakeConsumer(_ => Task.Factory.StartNew(() => { }));
            var queue = broker.DeclareQueue(queueName);
            var exchange = broker.DeclareDirectExchange(exchangeName);
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            broker.SubscribeByAtLeastOnce(queue, _ => { _.Consumes(consumer); });
            broker.Connect();
            model.Verify(_ => _.QueueDeclare(queueName,
                                             false,
                                             false,
                                             false,
                                             It.IsAny<IDictionary<String, Object>>()));
            model.Verify(_ => _.ExchangeDeclare(exchangeName,
                                                "direct",
                                                false,
                                                false,
                                                It.IsAny<IDictionary<String, Object>>()));
            model.Verify(_ => _.QueueBind(queueName,
                                          exchangeName,
                                          routingKey,
                                          It.IsAny<IDictionary<String, Object>>()));
        }

        internal class BrokerWrapper : Broker
        {
            private readonly RabbitMQ.Client.IConnection _connection;
            private readonly IModel _model;

            internal BrokerWrapper(RabbitMQ.Client.IConnection connection,
                                   IModel model,
                                   EnvironmentConfiguration configuration)
                : base(configuration)
            {
                _connection = connection;
                _model = model;
            }

            protected internal override RabbitMQ.Client.IConnection CreateConnection()
            {
                return _connection;
            }

            protected internal override IModel CreateInboundModel(RabbitMQ.Client.IConnection connection,
                                                                  UInt32 prefetchSize,
                                                                  UInt16 prefetchCount)
            {
                return _model;
            }
        }
    }
}