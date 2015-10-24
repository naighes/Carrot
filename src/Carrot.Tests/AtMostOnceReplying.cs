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
        [Fact]
        public void AcknowledgeThrows()
        {
            const Int64 deliveryTag = 1234L;
            var model = new Mock<IModel>();
            model.Setup(_ => _.BasicAck(deliveryTag, false)).Throws(new Exception());
            var builder = new Mock<IConsumedMessageBuilder>();
            var configuration = new SubscriptionConfiguration();
            var consumer = new AtMostOnceConsumerWrapper(model.Object, builder.Object, configuration);
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
                                               SubscriptionConfiguration configuration)
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