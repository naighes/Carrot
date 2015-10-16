using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Tests
{
    public class SampleConsumer : Consumer<Foo>
    {
        public override Task Consume(Message<Foo> message)
        {
            throw new NotImplementedException();
        }
    }

    public class Foo
    {
    }
}