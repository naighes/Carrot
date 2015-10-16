using System;
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

    public class Foo
    {
    }

    public class OuterConsumerTests
    {
        [Fact]
        public void FactMethodName()
        {
            var consumer = new FakeConsumer(message => { throw new Exception(); });
            //var outer = new OuterConsumer(consumer);
            //outer.Consume(new ConsumedMessage(null, null, 0, false)).Wait();
        }
    }
}