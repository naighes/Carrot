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
            var exchange = channel.DeclareDirectExchange("source_exchange");
            var queue = channel.DeclareQueue("my_test_queue");
            channel.DeclareExchangeBinding(exchange, queue, "routing_key");
            channel.SubscribeByAtLeastOnce(queue, _ => _.Consumes(new FooConsumer()));

            var connection = channel.Connect();

            connection.PublishAsync(new OutboundMessage<Foo>(new Foo { Bar = 2 }), exchange);

            Console.ReadLine();

            connection.Dispose();
        }
    }
}