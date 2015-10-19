using System;
using System.Threading.Tasks;
using Carrot.Messages;
using Carrot.Messaging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class AtLeastOnceReplying
    {
        [Fact]
        public void ReplyOnSuccess()
        {
            const Int64 deliveryTag = 1234L;
            var args = new BasicDeliverEventArgs
                           {
                               DeliveryTag = deliveryTag,
                               BasicProperties = new BasicProperties()
                           };
            var configuration = new SubscriptionConfiguration();
            var builder = new Mock<IConsumedMessageBuilder>();
            var message = new FakeConsumedMessage(args, _ => new Success(_));
            builder.Setup(_ => _.Build(args)).Returns(message);
            var model = new Mock<IModel>();
            var consumer = new AtLeastOnceConsumerWrapper(model.Object,
                                                          builder.Object,
                                                          configuration);
            consumer.CallConsumeInternal(args).Wait();
            model.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        internal class FakeConsumedMessage : ConsumedMessageBase
        {
            private readonly Func<ConsumedMessageBase, AggregateConsumingResult> _result;

            public FakeConsumedMessage(BasicDeliverEventArgs args,
                                       Func<ConsumedMessageBase, AggregateConsumingResult> result)
                : base(args)
            {
                _result = result;
            }

            internal override Object Content
            {
                get { throw new NotImplementedException(); }
            }

            internal override Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
            {
                return Task.FromResult(_result(this));
            }

            internal override Boolean Match(Type type)
            {
                throw new NotImplementedException();
            }
        }

        internal class AtLeastOnceConsumerWrapper : AtLeastOnceConsumer
        {
            internal AtLeastOnceConsumerWrapper(IModel model, 
                                                IConsumedMessageBuilder builder, 
                                                SubscriptionConfiguration configuration)
                : base(model, builder, configuration)
            {
            }

            internal Task CallConsumeInternal(BasicDeliverEventArgs args)
            {
                return ConsumeInternal(args);
            }
        }
    }
}