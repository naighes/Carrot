using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client;
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
            _configuration = new ConsumingConfiguration(new Mock<IChannel>().Object, null);
        }

        [Fact]
        public void AcknowledgeThrows()
        {
            const Int64 deliveryTag = 1234L;
            var model = new Mock<IModel>();
            model.Setup(_ => _.BasicAck(deliveryTag, false)).Throws(new Exception());
            var builder = new Mock<IConsumedMessageBuilder>();
            var consumer = new AtMostOnceConsumerWrapper(model.Object, builder.Object, _configuration);
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
            internal AtMostOnceConsumerWrapper(IModel model,
                                               IConsumedMessageBuilder builder,
                                               ConsumingConfiguration configuration)
                : base(model, builder, configuration)
            {
            }

            internal Task CallConsumeInternal(BasicDeliverEventArgs args)
            {
                return ConsumeInternalAsync(args);
            }
        }
    }
}