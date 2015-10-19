using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Messages;
using Carrot.Messaging;
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
            var result = new ConsumedMessage(new Foo(), null, 0, false, 0L).ConsumeAsync(configuration)
                                                                           .Result;
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
            var result = new ConsumedMessage(new Foo(), null, 0, false, 0L).ConsumeAsync(configuration)
                                                                           .Result;
            var actual = Assert.IsType<Failure>(result);
            Assert.Equal(1, actual.Exceptions.Length);
            Assert.Equal(message, 
                         actual.Exceptions
                               .First()
                               .Message);
            Assert.Equal(1, consumer.Errors.Count);
            Assert.Equal(message, consumer.Errors.First().Message);
        }

        [Fact]
        public void OnCorruptedMessage()
        {
            var result = new CorruptedMessage(null, 0, false, 0L).ConsumeAsync(null)
                                                                 .Result;
            var actual = Assert.IsType<CorruptedMessageFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        [Fact]
        public void OnUnresolvedMessage()
        {
            var result = new UnresolvedMessage(null, 0, false, 0L).ConsumeAsync(null)
                                                                  .Result;
            var actual = Assert.IsType<UnresolvedMessageFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        [Fact]
        public void OnUnsupportedMessage()
        {
            var result = new UnsupportedMessage(null, 0, false, 0L).ConsumeAsync(null)
                                                                   .Result;
            var actual = Assert.IsType<UnsupportedMessageFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }
    }

    public class Foo { }

    public class Bar { }

    internal class FakeConsumedMessage : ConsumedMessage
    {
        internal FakeConsumedMessage(Object content, 
                                     String messageId, 
                                     UInt64 deliveryTag, 
                                     Boolean redelivered,
                                     Int64 timestamp)
            : base(content, messageId, deliveryTag, redelivered, timestamp)
        {
        }

        internal override Boolean Match(Type type)
        {
            return true;
        }
    }

    internal class FakeConsumer : Consumer<Foo>
    {
        private readonly Func<Message<Foo>, Task> _func;

        internal readonly IList<Exception> Errors = new List<Exception>();

        public FakeConsumer(Func<Message<Foo>, Task> func)
        {
            _func = func;
        }

        public override void OnError(Exception exception)
        {
            base.OnError(exception);

            this.Errors.Add(exception);
        }

        public override Task Consume(Message<Foo> message)
        {
            return _func(message);
        }
    }
}