using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Tests
{
    public class SampleConsumer : Consumer<Foo>
    {
        public override Task Consume(Foo message)
        {
            return null;
        }
    }

    public class Foo
    {
    }
}