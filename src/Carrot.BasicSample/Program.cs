using System;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    public class Program
    {
        private static void Main()
        {
            var channel = Channel.New("amqp://guest:guest@localhost:5672/",
                                      new MessageBindingResolver(typeof(Foo).Assembly));
            var exchange = Exchange.Direct("source_exchange");

            var queue = Queue.New("my_test_queue");
            channel.Bind(queue, exchange)
                   .SubscribeByAtLeastOnce(_ =>
                                           {
                                               _.Consumes(new FooConsumer());
                                           });

            var connection = channel.Connect();

            connection.PublishAsync(new OutboundMessage<Foo>(new Foo { Bar = 2 }), exchange);

            Console.ReadLine();

            connection.Dispose();
        }
    }
}