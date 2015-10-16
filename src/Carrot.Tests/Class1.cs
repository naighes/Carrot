using System;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Messages;
using Carrot.Messaging;
using Xunit;

namespace Carrot.Tests
{
    public class FakeConsumer : Consumer<Foo>
    {
        private readonly Func<Message<Foo>, Task> _func;

        public FakeConsumer(Func<Message<Foo>, Task> func)
        {
            _func = func;
        }

        public override Task Consume(Message<Foo> message)
        {
            return _func(message);
        }
    }

    public class Foo { }

    public class Consuming
    {
        [Fact]
        public void NestedConsumerConsumingSuccesfully()
        {
            var configuration = new SubscriptionConfiguration();
            configuration.Consumes(new FakeConsumer(_ => Task.Factory.StartNew(() => { })));
            var result = new ConsumedMessage(new Foo(), null, 0, false).ConsumeAsync(configuration)
                                                                       .Result;
            Assert.IsType<Success>(result);
        }

        [Fact]
        public void NestedConsumerThrows()
        {
            const String message = "boom";
            var configuration = new SubscriptionConfiguration();
            configuration.Consumes(new FakeConsumer(_ => { throw new Exception(message); }));
            var result = new ConsumedMessage(new Foo(), null, 0, false).ConsumeAsync(configuration)
                                                                       .Result;
            var actual = Assert.IsType<Failure>(result);
            Assert.Equal(1, actual.Exceptions.Length);
            Assert.Equal(message, 
                         actual.Exceptions
                               .First()
                               .Message);
        }

        [Fact]
        public void OnCorruptedMessage()
        {
            var result = new CorruptedMessage(null, 0, false).ConsumeAsync(null)
                                                             .Result;
            var actual = Assert.IsType<CorruptedMessageFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        [Fact]
        public void OnUnresolvedMessage()
        {
            var result = new UnresolvedMessage(null, 0, false).ConsumeAsync(null)
                                                              .Result;
            var actual = Assert.IsType<UnresolvedMessageFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        [Fact]
        public void OnUnsupportedMessage()
        {
            var result = new UnsupportedMessage(null, 0, false).ConsumeAsync(null)
                                                               .Result;
            var actual = Assert.IsType<UnsupportedMessageFailure>(result);
            Assert.Equal(0, actual.Exceptions.Length);
        }

        internal class FakeMessage : ConsumedMessage
        {
            internal FakeMessage(Object content, String messageId, UInt64 deliveryTag, Boolean redelivered)
                : base(content, messageId, deliveryTag, redelivered)
            {
            }

            internal override Boolean Match(Type type)
            {
                return true;
            }
        }
    }
}