using System;
using System.Collections.Generic;
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
            var model = BuildModel(deliveryTag,
                                   _ => new Success(_),
                                   _configuration);
            model.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        [Fact]
        public void ReplyOnConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildModel(deliveryTag,
                                   _ => new ConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.BasicNack(deliveryTag, false, true));
        }

        [Fact]
        public void ReplyOnReiteratedConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildModel(deliveryTag,
                                   _ => new ReiteratedConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        [Fact]
        public void ReplyOnCorruptedMessageConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildModel(deliveryTag,
                                   _ => new CorruptedMessageConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        [Fact]
        public void ReplyOnUnresolvedMessageConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildModel(deliveryTag,
                                   _ => new UnresolvedMessageConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        [Fact]
        public void ReplyOnUnsupportedMessageConsumingFailure()
        {
            const Int64 deliveryTag = 1234L;
            var model = BuildModel(deliveryTag,
                                   _ => new UnsupportedMessageConsumingFailure(_),
                                   _configuration);
            model.Verify(_ => _.BasicAck(deliveryTag, false));
        }

        private static Mock<IModel> BuildModel(UInt64 deliveryTag,
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
            var model = new Mock<IModel>();
            var consumer = new AtLeastOnceConsumerWrapper(model.Object, default(Queue), builder.Object, configuration);
            consumer.CallConsumeInternal(args).Wait();
            return model;
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
            internal AtLeastOnceConsumerWrapper(IModel model,
                                                Queue queue,
                                                IConsumedMessageBuilder builder,
                                                ConsumingConfiguration configuration)
                : base(model, queue, builder, configuration)
            {
            }

            internal Task CallConsumeInternal(BasicDeliverEventArgs args)
            {
                return ConsumeInternalAsync(args);
            }
        }
    }
}