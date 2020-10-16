using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client.Events;
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
            _configuration.Consumes(new FakeConsumer(consumedMessage => throw new Exception()));
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
        public void OnReiteratedErrorFallbackFailure()
        {
            var inboundChannel = new Mock<IInboundChannel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs(true);
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var outboundChannel = new Mock<IOutboundChannel>();
            strategy.Setup(_ => _.Apply(outboundChannel.Object, message))
                .ReturnsAsync(new FallbackAppliedFailure(new Exception()));
            var consumer = new AtLeastOnceConsumerWrapper(inboundChannel.Object,
                outboundChannel.Object,
                default(Queue),
                builder.Object,
                _configuration);

            var result = consumer.CallConsumeInternalAsync(args).Result;
            
            Assert.IsType<ReiteratedConsumingFailure>(result);
            outboundChannel
                .Verify(c =>
                    c.PublishAsync(It.IsAny<OutboundMessage<FakeConsumedMessage>>(), It.IsAny<Exchange>(),
                        It.IsAny<string>()), Times.Never);
            outboundChannel
                .Verify(c =>
                    c.PublishAsync(It.IsAny<OutboundMessage<FakeConsumedMessage>>(), It.IsAny<Exchange>(),
                        It.IsAny<string>()), Times.Never);
            inboundChannel
                .Verify(c =>
                    c.NegativeAcknowledge(message.Args.DeliveryTag, It.Is<bool>(boolean => boolean.Equals(true))));
        }

        [Fact]
        public async Task DeadLetterExchangeStrategy()
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
            outboundChannel.Setup(c =>
                c.ForwardAsync(message, It.Is<Exchange>(e => e.Name == dleName),
                    string.Empty)).ReturnsAsync(SuccessfulPublishing.FromBasicProperties(message.Args.BasicProperties));

            var applied = await strategy.Apply(outboundChannel.Object, message);

            Assert.True(applied.Success);
            outboundChannel.Verify();
        }

        [Fact]
        public async Task DeadLetterExchangeStrategyForwardError()
        {
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
            var message = new FakeConsumedMessage(null, FakeBasicDeliverEventArgs());
            var outboundChannel = new Mock<IOutboundChannel>();
            outboundChannel
                .Setup(c => c.ForwardAsync(message, It.Is<Exchange>(e => e.Name == dleName), string.Empty))
                .ReturnsAsync(new FailurePublishing(new Exception()))
                .Verifiable();

            var applied = await strategy.Apply(outboundChannel.Object, message);

            Assert.False(applied.Success);
            outboundChannel.Verify();
        }

        private static BasicDeliverEventArgs FakeBasicDeliverEventArgs(bool redelivered = false)
        {
            return new BasicDeliverEventArgs
            {
                BasicProperties = BasicPropertiesStubber.Stub(_ => _.Headers = new Dictionary<String, Object>()),
                Redelivered = redelivered
            };
        }

        private class AtLeastOnceConsumerWrapper : AtLeastOnceConsumer
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