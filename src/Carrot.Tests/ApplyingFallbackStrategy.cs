using System;
using System.Collections.Generic;
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
        [Fact]
        public void OnSuccess()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            var result = message.ConsumeAsync(new SubscriptionConfiguration(strategy.Object)).Result;
            Assert.IsType<Success>(result);
            result.Reply(model.Object);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Never);
        }

        [Fact]
        public void OnError()
        {
            var model = new Mock<IModel>();
            var strategy = new Mock<IFallbackStrategy>();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Object(), args);
            var configuration = new SubscriptionConfiguration(strategy.Object);
            configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var result = message.ConsumeAsync(configuration).Result;
            Assert.IsType<ConsumingFailure>(result);
            result.Reply(model.Object);
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
            var configuration = new SubscriptionConfiguration(strategy.Object);
            configuration.Consumes(new FakeConsumer(consumedMessage => { throw new Exception(); }));
            var result = message.ConsumeAsync(configuration).Result;
            Assert.IsType<ReiteratedConsumingFailure>(result);
            result.Reply(model.Object);
            strategy.Verify(_ => _.Apply(model.Object, message), Times.Once);
        }

        [Fact]
        public void DeadLetterExchangeStrategy()
        {
            const String expected = "source_exchange-DeadLetter";
            var args = FakeBasicDeliverEventArgs();
            args.Exchange = "source_exchange";
            var message = new FakeConsumedMessage(null, args);
            var strategy = new DeadLetterStrategy(_ => String.Format("{0}-DeadLetter", _));
            var model = new Mock<IModel>();
            strategy.Apply(model.Object, message);
            model.Verify(_ => _.BasicPublish(expected,
                                             String.Empty,
                                             args.BasicProperties,
                                             args.Body),
                         Times.Once);
            model.Verify(_ => _.ExchangeDeclare(expected, "direct", true));
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
    }
}