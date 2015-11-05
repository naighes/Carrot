using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Configuration;
using Moq;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class SubscribingConfiguration
    {
        private readonly ConsumingConfiguration _configuration;

        public SubscribingConfiguration()
        {
            _configuration = new ConsumingConfiguration(new Mock<IChannel>().Object, null);
        }

        [Fact]
        public void OneSubscription()
        {
            _configuration.Consumes(new FakeConsumer(_ => null));
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Foo(), args);
            var subscriptions = _configuration.FindSubscriptions(message);
            Assert.Equal(1, subscriptions.Count());
        }

        [Fact]
        public void TwoSubscriptionsForTheSameMessage()
        {
            _configuration.Consumes(new FakeConsumer(_ => null));
            _configuration.Consumes(new FakeConsumer(_ => null));
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Foo(), args);
            var subscriptions = _configuration.FindSubscriptions(message);
            Assert.Equal(2, subscriptions.Count());
        }

        [Fact]
        public void NoSubscriptions()
        {
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Foo(), args);
            var subscriptions = _configuration.FindSubscriptions(message);
            Assert.Equal(0, subscriptions.Count());
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