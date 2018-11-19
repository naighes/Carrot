using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class AtMostOnceReplying
    {
        private readonly ConsumingConfiguration _configuration;

        public AtMostOnceReplying()
        {
            _configuration = new ConsumingConfiguration(new Mock<IBroker>().Object, default(Queue));
        }

        [Fact]
        public void AcknowledgeThrows()
        {
            const Int64 deliveryTag = 1234L;
            var inboundChannel = new Mock<IInboundChannel>();
            inboundChannel.Setup(_ => _.Acknowledge(deliveryTag)).Throws(new Exception());
            var builder = new Mock<IConsumedMessageBuilder>();
            var consumer = new AtMostOnceConsumerWrapper(inboundChannel.Object,
                                                         new Mock<IOutboundChannelPool>().Object,
                                                         default(Queue),
                                                         builder.Object,
                                                         _configuration);
            var args = new BasicDeliverEventArgs
                           {
                               DeliveryTag = deliveryTag,
                               BasicProperties = new BasicProperties()
                           };
            Assert.Throws<Exception>(() => consumer.CallConsumeInternal(args).Wait());
            builder.Verify(_ => _.Build(args), Times.Never);
        }

        internal class AtMostOnceConsumerWrapper : AtMostOnceConsumer
        {
            internal AtMostOnceConsumerWrapper(IInboundChannel inboundChannel,
                                               IOutboundChannelPool outboundChannelPool,
                                               Queue queue,
                                               IConsumedMessageBuilder builder,
                                               ConsumingConfiguration configuration)
                : base(inboundChannel, outboundChannelPool, queue, builder, configuration)
            {
            }

            internal Task CallConsumeInternal(BasicDeliverEventArgs args)
            {
                return ConsumeInternalAsync(args);
            }
        }
    }
}