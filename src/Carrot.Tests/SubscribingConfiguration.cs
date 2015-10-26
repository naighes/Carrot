using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Configuration;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class SubscribingConfiguration
    {
        [Fact]
        public void OneSubscription()
        {
            var configuration = new SubscriptionConfiguration();
            configuration.Consumes(new FakeConsumer(_ => null));
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Foo(), args);
            var subscriptions = configuration.FindSubscriptions(message);
            Assert.Equal(1, subscriptions.Count());
        }

        [Fact]
        public void NoSubscriptions()
        {
            var configuration = new SubscriptionConfiguration();
            var args = FakeBasicDeliverEventArgs();
            var message = new FakeConsumedMessage(new Foo(), args);
            var subscriptions = configuration.FindSubscriptions(message);
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