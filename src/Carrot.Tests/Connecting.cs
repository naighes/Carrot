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
            var configuration = new ChannelConfiguration();
            configuration.GeneratesMessageIdBy(new Mock<INewId>().Object);
            configuration.ResolveMessageTypeBy(new Mock<IMessageTypeResolver>().Object);
            var channel = new ChannelWrapper(new Mock<IConnection>().Object,
                                             model.Object,
                                             configuration);
            var consumer = new FakeConsumer(_ => Task.Factory.StartNew(() => { }));
            var queue = channel.DeclareQueue(queueName);
            var exchange = channel.DeclareDirectExchange(exchangeName);
            channel.DeclareExchangeBinding(exchange, queue, routingKey);
            channel.SubscribeByAtLeastOnce(queue, _ => { _.Consumes(consumer); });
            channel.Connect();
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

        internal class ChannelWrapper : Channel
        {
            private readonly IConnection _connection;
            private readonly IModel _model;

            internal ChannelWrapper(IConnection connection,
                                    IModel model,
                                    ChannelConfiguration configuration)
                : base(configuration)
            {
                _connection = connection;
                _model = model;
            }

            protected internal override IConnection CreateConnection()
            {
                return _connection;
            }

            protected internal override IModel CreateOutboundModel(IConnection connection)
            {
                return _model;
            }

            protected internal override IModel CreateInboundModel(IConnection connection,
                                                                  UInt32 prefetchSize,
                                                                  UInt16 prefetchCount)
            {
                return _model;
            }
        }
    }
}