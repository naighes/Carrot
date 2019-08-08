using System;
using System.Reflection;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    public class Program
    {
        private static void Main()
        {
            const String routingKey = "routing_key";
            const String endpointUrl = "amqp://guest:guest@localhost:5672/";
            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Foo).GetTypeInfo().Assembly);

            var broker = Broker.New(_ =>
                                    {
                                        _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                                        _.ResolveMessageTypeBy(resolver);
                                    });

            var exchange = broker.DeclareDirectExchange("source_exchange");
            var queue = broker.DeclareQueue("my_test_queue");
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            var fooExchange = broker.DeclareDirectExchange("foo_exchange");
            var fooQueue = broker.DeclareQueue("foo_queue");
            broker.DeclareExchangeBinding(fooExchange, fooQueue, routingKey);

            // competing consumer scenario
            broker.SubscribeByAtLeastOnce(queue,
                                          _ =>
                                          {
                                              _.FallbackBy((c, a) => DeadLetterStrategy.New(c, a, x => $"{x}-Error"));
                                              _.Consumes(new PublishingFooConsumer(fooExchange, routingKey));
                                          });
            broker.SubscribeByAtLeastOnce(queue,
                                          _ =>
                                          {
                                              _.FallbackBy((c, a) => DeadLetterStrategy.New(c, a, x => $"{x}-Error"));
                                              _.Consumes(new FooConsumer());
                                          });
            var connection = broker.Connect();

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