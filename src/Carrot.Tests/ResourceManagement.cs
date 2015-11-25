using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace Carrot.Tests
{
    public class ResourceManagement
    {
        [Fact]
        public void OnConnectionDisposed()
        {
            var consumerModel1 = new Mock<IModel>();
            var consumer1 = new FakeConsumerBase(consumerModel1.Object, null, null);
            var consumerModel2 = new Mock<IModel>();
            var consumer2 = new FakeConsumerBase(consumerModel2.Object, null, null);
            var outboundModel = new Mock<IModel>();
            var connection = new Mock<IConnection>();
            var amqpConnection = new AmqpConnection(connection.Object,
                                                    new List<ConsumerBase> { consumer1, consumer2 },
                                                    new ReliableOutboundChannel(outboundModel.Object),
                                                    null,
                                                    null);
            amqpConnection.Dispose();

            connection.Verify(_ => _.Dispose(), Times.Once);
            outboundModel.Verify(_ => _.Dispose(), Times.Once);
            consumerModel1.Verify(_ => _.Dispose(), Times.Once);
            consumerModel2.Verify(_ => _.Dispose(), Times.Once);
        }

        internal class FakeConsumerBase : ConsumerBase
        {
            public FakeConsumerBase(IModel model, IConsumedMessageBuilder builder, ConsumingConfiguration configuration)
                : base(model, builder, configuration)
            {
            }

            protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
            {
                throw new NotImplementedException();
            }
        }
    }
}