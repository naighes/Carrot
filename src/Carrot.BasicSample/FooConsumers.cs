using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    internal class FooConsumer1 : Consumer<Foo>
    {
        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name);
            });
        }
    }

    internal class FooConsumer2 : Consumer<Foo>
    {
        public override Task ConsumeAsync(ConsumedMessage<Foo> message)
        {
            return Task.Factory.StartNew(() =>
            {
                Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                  message.ConsumerTag,
                                  message.Headers.MessageId,
                                  GetType().Name);
            });
        }
    }
}