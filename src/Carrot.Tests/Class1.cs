using System;
using System.Threading.Tasks;
using Carrot.Messages;
using Carrot.Messaging;
using Xunit;

namespace Carrot.Tests
{
    using System.Linq;

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

    public class Foo
    {
    }

    public class OuterConsumerTests
    {
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