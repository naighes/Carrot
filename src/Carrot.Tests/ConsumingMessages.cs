using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class ConsumingMessages
    {
        [Fact]
        public void ConsumingSuccesfully()
        {
            var configuration = new SubscriptionConfiguration();
            configuration.Consumes(new FakeConsumer(_ => Task.Factory.StartNew(() => { })));
            var result = new ConsumedMessage(new Foo(),
                                             FakeBasicDeliverEventArgs()).ConsumeAsync(configuration).Result;
            Assert.IsType<Success>(result);
        }

        [Fact]
        public void Throws()
        {
            const String message = "boom";
            var exception = new Exception(message);
            var configuration = new SubscriptionConfiguration();
            var consumer = new FakeConsumer(_ => { throw exception; });
            configuration.Consumes(consumer);
            var result = new ConsumedMessage(new Foo(),
                                             FakeBasicDeliverEventArgs()).ConsumeAsync(configuration).Result;
            var actual = Assert.IsType<ConsumingFailure>(result);
            Assert.Equal(1, actual.Exceptions.Length);
            Assert.Equal(message, actual.Exceptions.First().Message);
            Assert.Equal(1, consumer.Errors.Count);
            Assert.Equal(message, consumer.Errors.First().Message);
        }

        [Fact]
        public void ThrowsOnRedelivered()
        {
            const String message = "boom";
            var exception = new Exception(message);
            var configuration = new SubscriptionConfiguration();
            var consumer = new FakeConsumer(_ => { throw exception; });
            configuration.Consumes(consumer);
            var result = new ConsumedMessage(new Foo(),
                                             new BasicDeliverEventArgs
                                                 {
                                                     Redelivered = true,
                                                     BasicProperties = new BasicProperties()
                                                 }).ConsumeAsync(configuration).Result;
            var actual = Assert.IsType<ReiteratedConsumingFailure>(result);
            Assert.Equal(1, actual.Exceptions.Length);
            Assert.Equal(message, actual.Exceptions.First().Message);
            Assert.Equal(1, consumer.Errors.Count);
            Assert.Equal(message, consumer.Errors.First().Message);
        }

        [Fact]
        public void OnCorruptedMessage()
        {
            var result = new CorruptedMessage(FakeBasicDeliverEventArgs()).ConsumeAsync(new SubscriptionConfiguration())
                                                                          .Result;
            var actual = Assert.IsType<CorruptedMessageConsumingFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        [Fact]
        public void OnUnresolvedMessage()
        {
            var result = new UnresolvedMessage(FakeBasicDeliverEventArgs()).ConsumeAsync(new SubscriptionConfiguration())
                                                                           .Result;
            var actual = Assert.IsType<UnresolvedMessageConsumingFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        [Fact]
        public void OnUnsupportedMessage()
        {
            var result = new UnsupportedMessage(FakeBasicDeliverEventArgs()).ConsumeAsync(new SubscriptionConfiguration())
                                                                            .Result;
            var actual = Assert.IsType<UnsupportedMessageConsumingFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        private static BasicDeliverEventArgs FakeBasicDeliverEventArgs()
        {
            return new BasicDeliverEventArgs
                       {
                           BasicProperties = new BasicProperties()
                       };
        }
    }

    [MessageBinding("urn:message:buzz", ExpiresAfter = 19)]
    public class Buzz { }

    [MessageBinding("urn:message:foo")]
    public class Foo { }

    public class Bar { }

    internal class FakeConsumedMessage : ConsumedMessage
    {
        internal FakeConsumedMessage(Object content, BasicDeliverEventArgs args)
            : base(content, args)
        {
        }

        internal override Boolean Match(Type type)
        {
            return true;
        }
    }

    internal class FakeConsumer : Consumer<Foo>
    {
        internal readonly IList<Exception> Errors = new List<Exception>();

        private readonly Func<ConsumedMessage<Foo>, Task> _func;

        public FakeConsumer(Func<ConsumedMessage<Foo>, Task> func)
        {
            _func = func;
        }

        public override void OnError(Exception exception)
        {
            base.OnError(exception);

            Errors.Add(exception);
        }

        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return _func(message);
        }
    }
}