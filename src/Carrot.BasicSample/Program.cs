using System;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = AmqpChannel.New("amqp://guest:guest@localhost:5672/",
                              new MessageBindingResolver(typeof(Foo).Assembly));
            var exchange = Exchange.Direct("source_exchange");

            channel.Bind("my_test_queue", exchange)
                   .SubscribeByAtLeastOnce(_ => { _.Consumes(new FooConsumer()); });

            channel.PublishAsync(new OutboundMessage<Foo>(new Foo { Bar = 2 }),
                     exchange);

            Console.ReadLine();

            channel.Dispose();
        }
    }
}
