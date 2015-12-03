using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Moq;
using RabbitMQ.Client;
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
            var model = new Mock<IModel>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            var strategy = new Mock<IFallbackStrategy>();
            _configuration.FallbackBy((c, q) => strategy.Object);
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var consumer = new AtLeastOnceConsumer(model.Object, default(Queue), builder.Object, _configuration);
            var result = consumer.ConsumeAsync(args).Result;
            Assert.IsType<Success>(result);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Never);
        }

        [Fact]
        public void OnError()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var consumer = new AtLeastOnceConsumer(model.Object, default(Queue), builder.Object, _configuration);
            var result = consumer.ConsumeAsync(args).Result;
            Assert.IsType<ConsumingFailure>(result);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Never);
        }

        [Fact]
        public void OnReiteratedError()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            args.Redelivered = true;
            var message = new FakeConsumedMessage(new Object(), args);
            _configuration.FallbackBy((c, q) => strategy.Object);
            _configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var builder = new Mock<IConsumedMessageBuilder>();
            builder.Setup(_ => _.Build(args)).Returns(message);
            var consumer = new AtLeastOnceConsumerWrapper(model.Object, default(Queue), builder.Object, _configuration);
            var result = consumer.CallConsumeInternalAsync(args).Result;
            Assert.IsType<ReiteratedConsumingFailure>(result);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Once);
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
            broker.Setup(_ => _.DeclareDurableDirectExchange(dleName)).Returns(new Exchange(dleName, "direct"));
            var strategy = DeadLetterStrategy.New(broker.Object,
                                                  queue,
                                                  _ => $"{_}-DeadLetter");
            var model = new Mock<IModel>();
            strategy.Apply(model.Object, message);
            model.Verify(_ => _.BasicPublish(dleName,
                                             String.Empty,
                                             true,
                                             false,
                                             It.Is<IBasicProperties>(__ => __.Persistent == true),
                                             args.Body),
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
            public AtLeastOnceConsumerWrapper(IModel model,
                                              Queue queue,
                                              IConsumedMessageBuilder builder,
                                              ConsumingConfiguration configuration)
                : base(model, queue, builder, configuration)
            {
            }

            internal Task<AggregateConsumingResult> CallConsumeInternalAsync(BasicDeliverEventArgs args)
            {
                return ConsumeInternalAsync(args);
            }
        }
    }
}