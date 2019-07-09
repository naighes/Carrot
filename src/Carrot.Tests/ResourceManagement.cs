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
            var consumerChannel1 = new Mock<IInboundChannel>();
            var consumer1 = new FakeConsumerBase(consumerChannel1.Object,
                                                 new Mock<IOutboundChannelPool>().Object,
                                                 default(Queue),
                                                 null,
                                                 null);
            var consumerChannel2 = new Mock<IInboundChannel>();
            var consumer2 = new FakeConsumerBase(consumerChannel2.Object,
                                                 new Mock<IOutboundChannelPool>().Object,
                                                 default(Queue),
                                                 null,
                                                 null);
            var outboundChannelPool = new Mock<IOutboundChannelPool>();
            var connection = new Mock<RabbitMQ.Client.IConnection>();
            var amqpConnection = new Connection(connection.Object,
                                                new List<ConsumerBase> { consumer1, consumer2 },
                                                outboundChannelPool.Object);
            amqpConnection.Dispose();

            connection.Verify(_ => _.Dispose(), Times.Once);
            outboundChannelPool.Verify(_ => _.Dispose(), Times.Once);
            consumerChannel1.Verify(_ => _.Dispose(), Times.Once);
            consumerChannel2.Verify(_ => _.Dispose(), Times.Once);
        }

        internal class FakeConsumerBase : ConsumerBase
        {
            public FakeConsumerBase(IInboundChannel inboundChannel,
                                    IOutboundChannelPool outboundChannelPool,
                                    Queue queue,
                                    IConsumedMessageBuilder builder,
                                    ConsumingConfiguration configuration)
                : base(inboundChannel, outboundChannelPool, queue, builder, configuration)
            {
            }

            protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
            {
                throw new NotImplementedException();
            }
        }
    }
}