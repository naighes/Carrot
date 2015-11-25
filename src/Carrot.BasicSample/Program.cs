using System;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    public class Program
    {
        private static void Main()
        {
            const String routingKey = "routing_key";
            const String endpointUrl = "amqp://guest:guest@localhost:5672/";
            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Foo).Assembly);

            var channel = Channel.New(_ =>
            {
                _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                _.ResolveMessageTypeBy(resolver);
            });

            var exchange = channel.DeclareDirectExchange("source_exchange");
            var queue = channel.DeclareQueue("my_test_queue");
            channel.DeclareExchangeBinding(exchange, queue, routingKey);
            channel.SubscribeByAtLeastOnce(queue, _ => _.Consumes(new FooConsumer1()));
            channel.SubscribeByAtLeastOnce(queue, _ => _.Consumes(new FooConsumer2()));
            var connection = channel.Connect();

            for (var i = 0; i < 5; i++)
            {
                var message = new OutboundMessage<Foo>(new Foo { Bar = i });
                connection.PublishAsync(message, exchange, routingKey);
            }

            Console.ReadLine();
            connection.Dispose();
        }
    }
}