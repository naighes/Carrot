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
            strategy.Verify(_ => _.Apply(model.Object), Times.Never);
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
            strategy.Verify(_ => _.Apply(model.Object), Times.Never);
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
            strategy.Verify(_ => _.Apply(model.Object), Times.Once);
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