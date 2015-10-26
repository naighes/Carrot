using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    class FooConsumer : Consumer<Foo>
    {
        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("received '{0}'",
                    message.Headers.MessageId);
            });
        }
    }
}