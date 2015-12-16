using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class AtLeastOnceReplying
    {
        private readonly ConsumingConfiguration _configuration;

        public AtLeastOnceReplying()
        {
            _configuration = new ConsumingConfiguration(new Mock<IBroker>().Object, default(Queue));
        }

        [Fact]
        public void ReplyOnSuccess()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildInboundChannel(deliveryTag,
                                   _ => new Success(_),
                                   _configuration);
            model.Verify(_ => _.Acknowledge(deliveryTag));
        }

        [Fact]
        public void ReplyOnConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildInboundChannel(deliveryTag,
                                   _ => new ConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.NegativeAcknowledge(deliveryTag, true));
        }

        [Fact]
        public void ReplyOnReiteratedConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildInboundChannel(deliveryTag,
                                   _ => new ReiteratedConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.Acknowledge(deliveryTag));
        }

        [Fact]
        public void ReplyOnCorruptedMessageConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildInboundChannel(deliveryTag,
                                   _ => new CorruptedMessageConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.Acknowledge(deliveryTag));
        }

        [Fact]
        public void ReplyOnUnresolvedMessageConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildInboundChannel(deliveryTag,
                                   _ => new UnresolvedMessageConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.Acknowledge(deliveryTag));
        }

        [Fact]
        public void ReplyOnUnsupportedMessageConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildInboundChannel(deliveryTag,
                                   _ => new UnsupportedMessageConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.Acknowledge(deliveryTag));
        }

        private static Mock<IInboundChannel> BuildInboundChannel(UInt64 deliveryTag,
                                                        Func<ConsumedMessageBase, AggregateConsumingResult> func,
                                                        ConsumingConfiguration configuration)
        {
            var args = new BasicDeliverEventArgs
                           {
                               DeliveryTag = deliveryTag,
                               BasicProperties = new BasicProperties()
                           };
            var builder = new Mock<IConsumedMessageBuilder>();
            var message = new FakeConsumedMessage(args, func);
            builder.Setup(_ => _.Build(args)).Returns(message);
            var channel = new Mock<IInboundChannel>();
            var consumer = new AtLeastOnceConsumerWrapper(channel.Object,
                                                          new Mock<IOutboundChannel>().Object,
                                                          default(Queue),
                                                          builder.Object,
                                                          configuration);
            consumer.CallConsumeInternal(args).Wait();
            return channel;
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

            internal override Task<AggregateConsumingResult> ConsumeAsync(IEnumerable<IConsumer> subscriptions)
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
            internal AtLeastOnceConsumerWrapper(IInboundChannel inboundChannel,
                                                IOutboundChannel outboundChannel,
                                                Queue queue,
                                                IConsumedMessageBuilder builder,
                                                ConsumingConfiguration configuration)
                : base(inboundChannel, outboundChannel, queue, builder, configuration)
            {
            }

            internal Task CallConsumeInternal(BasicDeliverEventArgs args)
            {
                return ConsumeInternalAsync(args);
            }
        }
    }
}