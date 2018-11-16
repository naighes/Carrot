using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
                                        //_.PublishBy(OutboundChannel.Reliable());
                                    });

            var exchange = broker.DeclareDirectExchange("source_exchange");
            var queue = broker.DeclareQueue("my_test_queue");
            broker.DeclareExchangeBinding(exchange, queue, routingKey);
            broker.SubscribeByAtLeastOnce(queue,
                                          _ =>
                                          {
                                              _.FallbackBy((c, a) => DeadLetterStrategy.New(c, a, x => $"{x}-Error"));
                                              _.Consumes(new FooConsumer1());
                                          });
            broker.SubscribeByAtLeastOnce(queue,
                                          _ =>
                                          {
                                              _.FallbackBy((c, a) => DeadLetterStrategy.New(c, a, x => $"{x}-Error"));
                                              _.Consumes(new FooConsumer2());
                                          });
            var connection = broker.Connect();

            var tasks = Enumerable.Range(0, 128).Select(i => Task.Run(() => DoPublish(connection, exchange, routingKey, 256)));
            Task.WhenAll(tasks);

            Console.ReadLine();
            connection.Dispose();
        }

        private static async Task DoPublish(IConnection connection, Exchange exchange, string routingKey, int count)
        {
            //Console.WriteLine("Before publishing all");
            await Task.WhenAll(Enumerable.Range(0, count).Select(i =>
                connection.PublishAsync(new OutboundMessage<Foo>(new Foo {Bar = i}), exchange, routingKey)));
            //Console.WriteLine("After publishing all");
        }
    }
}