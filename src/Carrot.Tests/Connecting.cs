using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Serialization;
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
            var channel = new ChannelWrapper(new Mock<IConnection>().Object,
                                             model.Object,
                                             "amqp://fake.url",
                                             new Mock<IDateTimeProvider>().Object,
                                             new Mock<INewId>().Object,
                                             new Mock<ISerializerFactory>().Object,
                                             new Mock<IMessageTypeResolver>().Object,
                                             0,
                                             0);
            var consumer = new FakeConsumer(_ => Task.Factory.StartNew(() => { }));
            channel.Bind(queueName, Exchange.Direct(exchangeName), routingKey)
                   .SubscribeByAtLeastOnce(_ => { _.Consumes(consumer); });
            channel.Connect();
            model.Verify(_ => _.QueueDeclare(queueName,
                                             true,
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
                                    String endpointUrl,
                                    IDateTimeProvider dateTimeProvider,
                                    INewId newId,
                                    ISerializerFactory serializerFactory,
                                    IMessageTypeResolver resolver,
                                    UInt32 prefetchSize,
                                    UInt16 prefetchCount)
                : base(endpointUrl,
                       dateTimeProvider,
                       newId,
                       serializerFactory,
                       resolver,
                       prefetchSize,
                       prefetchCount)
            {
                _connection = connection;
                _model = model;
            }

            protected internal override IConnection CreateConnection()
            {
                return _connection;
            }

            protected internal override IModel CreateModel(IConnection connection,
                                                           UInt32 prefetchSize,
                                                           UInt16 prefetchCount)
            {
                return _model;
            }
        }
    }
}