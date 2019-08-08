using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class ApplyingFallbackStrategy
    {
        private readonly ConsumingConfiguration _configuration;

        public ApplyingFallbackStrategy()
        {
            _configuration = new ConsumingConfiguration(new Mock<IBroker>().Object, default(Queue));
        }

        [Fact]
        public void OnSuccess()
        {
            var inboundChannel = new Mock<IInboundChannel>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            var strategy = new Mock<IFallbackStrategy>();
            _configuration.FallbackBy((c, q) => strategy.Object);
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var outboundChannel = new Mock<IOutboundChannel>().Object;
            var consumer = new AtLeastOnceConsumer(inboundChannel.Object,
                                                   outboundChannel,
                                                   default(Queue),
                                                   builder.Object,
                                                   _configuration);
            var result = consumer.ConsumeAsync(args,
                                               outboundChannel)
                                 .Result;
            Assert.IsType<Success>(result);
            strategy.Verify(_ => _.Apply(outboundChannel, message), Times.Never);
        }

        [Fact]
        public void OnError()
        {
            var inboundChannel = new Mock<IInboundChannel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var outboundChannel = new Mock<IOutboundChannel>().Object;
            var consumer = new AtLeastOnceConsumer(inboundChannel.Object,
                                                   outboundChannel,
                                                   default(Queue),
                                                   builder.Object,
                                                   _configuration);
            var result = consumer.ConsumeAsync(args,
                                               outboundChannel)
                                 .Result;
            Assert.IsType<ConsumingFailure>(result);
            strategy.Verify(_ => _.Apply(outboundChannel, message), Times.Never);
        }

        [Fact]
        public void OnReiteratedError()
        {
            var inboundChannel = new Mock<IInboundChannel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            args.Redelivered = true;
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var outboundChannel = new Mock<IOutboundChannel>().Object;
            var consumer = new AtLeastOnceConsumerWrapper(inboundChannel.Object,
                                                          outboundChannel,
                                                          default(Queue),
                                                          builder.Object,
                                                          _configuration);
            var result = consumer.CallConsumeInternalAsync(args).Result;
            Assert.IsType<ReiteratedConsumingFailure>(result);
            strategy.Verify(_ => _.Apply(outboundChannel, message), Times.Once);
        }

        [Fact]
        public void DeadLetterExchangeStrategy()
        {
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(null, args);
            var broker = new Mock<IBroker>();
            var queue = new Queue("queue_name");
            Func<String, String> f = _ => $"{_}-DeadLetter";
            var dleName = f(queue.Name);
            broker.Setup(_ => _.DeclareDurableDirectExchange(It.Is<String>(__ => __ == dleName),
                                                             It.IsAny<IDictionary<String, Object>>()))
                  .Returns(new Exchange(dleName, "direct"));
            var strategy = DeadLetterStrategy.New(broker.Object,
                                                  queue,
                                                  _ => $"{_}-DeadLetter");
            var outboundChannel = new Mock<IOutboundChannel>();
            strategy.Apply(outboundChannel.Object, message);
            outboundChannel.Verify(_ => _.ForwardAsync(message,
                                                       It.Is<Exchange>(__ => __.Name == dleName),
                                                       String.Empty),
                                   Times.Once);
        }

        private static BasicDeliverEventArgs FakeBasicDeliverEventArgs()
        {
            return new BasicDeliverEventArgs
                       {
                           BasicProperties = new BasicProperties
                                                 {
                                                     Headers = new Dictionary<String, Object>()
                                                 }
                       };
        }

        internal class AtLeastOnceConsumerWrapper : AtLeastOnceConsumer
        {
            public AtLeastOnceConsumerWrapper(IInboundChannel inboundChannel,
                                              IOutboundChannel outboundChannel,
                                              Queue queue,
                                              IConsumedMessageBuilder builder,
                                              ConsumingConfiguration configuration)
                : base(inboundChannel, outboundChannel, queue, builder, configuration)
            {
            }

            internal Task<AggregateConsumingResult> CallConsumeInternalAsync(BasicDeliverEventArgs args)
            {
                return ConsumeInternalAsync(args);
            }
        }
    }
}