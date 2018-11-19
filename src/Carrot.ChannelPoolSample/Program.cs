using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    public class Program
    {
        const string LoremIpsum = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin in nisi turpis. Ut dui diam, laoreet tincidunt quam vel, efficitur rhoncus dui. Vivamus vel ipsum velit. Fusce leo libero, imperdiet pharetra venenatis quis, semper et urna. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Pellentesque quis risus cursus, bibendum urna at, finibus tellus. Nam tempor dui nec tincidunt ultrices. Fusce lobortis tincidunt feugiat. Proin eu risus et augue feugiat molestie id finibus risus. In ultricies commodo tellus eu mollis. Fusce pharetra imperdiet nulla, mattis scelerisque augue rhoncus a. In in nunc felis. Nulla facilisi. Integer auctor aliquet sapien vitae tincidunt.

Etiam hendrerit lobortis mi. Proin posuere dictum massa, vitae sodales massa tempus ac. Vivamus vestibulum dolor eros, non laoreet mauris cursus vel. Ut id nibh molestie, ultrices ex a, auctor turpis. Pellentesque id ex eu velit tincidunt congue interdum sit amet urna. Sed fermentum sit amet magna a facilisis. Curabitur in sapien malesuada, commodo felis at, pharetra nisl. Mauris lacinia sapien a maximus tempus. Suspendisse semper varius quam id facilisis. Praesent consequat tincidunt odio, ac eleifend ipsum iaculis quis.

Donec pellentesque ligula sed est condimentum efficitur. Phasellus blandit gravida tempus. Vestibulum eget diam est. Vivamus a maximus neque, ac posuere tellus. Maecenas pharetra eros sem, eget ultrices velit porttitor eget. Suspendisse condimentum, eros sit amet ultrices tincidunt, sapien justo vehicula tellus, in tristique neque dui nec orci. Aenean feugiat, urna ut molestie consectetur, ligula ipsum sollicitudin arcu, ultrices bibendum odio odio ac magna. Cras id elementum felis. Sed eget erat tristique, ullamcorper tortor vitae, ornare felis. Suspendisse dictum velit nec risus consectetur tempor. Aenean et tortor sed mi scelerisque volutpat. Duis sodales vestibulum diam, efficitur placerat mi consectetur non. Nullam in turpis sit amet tellus tempus semper eget et justo. Ut gravida ipsum in lorem luctus gravida. Praesent non lorem non tortor ultricies viverra.

Vivamus eros ipsum, sagittis vitae dui vel, porttitor fermentum lorem. Pellentesque quis tellus at metus cursus molestie. Fusce luctus tellus a est malesuada congue. Nullam efficitur ipsum id lobortis bibendum. Ut hendrerit ut risus id fringilla. Suspendisse bibendum augue ut blandit ultrices. Fusce id dolor at elit semper semper.

Vestibulum in vehicula ipsum. Aliquam efficitur diam quis eros varius, at placerat nunc condimentum. Fusce ante nibh, gravida vitae molestie condimentum, gravida ac justo. Ut posuere fermentum tortor, eget pretium mauris ultrices at. Curabitur consequat auctor massa, vel eleifend libero tristique et. Curabitur dictum venenatis malesuada. Ut placerat turpis neque, quis ornare magna lobortis ac. In pellentesque nulla justo, vitae fringilla tortor faucibus sed. Integer vel nisl condimentum, aliquam sem eget, interdum diam. Aliquam gravida pharetra libero eget feugiat. Vivamus id neque et lectus rhoncus convallis.";

        private static void Main()
        {
            const String routingKey = "routing_key";
            const String endpointUrl = "amqp://guest:guest@localhost:5672/";
            IMessageTypeResolver resolver = new MessageBindingResolver(typeof(Foo).GetTypeInfo().Assembly);

            var broker = Broker.New(_ =>
                                    {
                                        _.Endpoint(new Uri(endpointUrl, UriKind.Absolute));
                                        _.ResolveMessageTypeBy(resolver);
                                        _.PublishBy(OutboundChannel.Reliable());
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
            await Task.WhenAll(Enumerable.Range(0, count).Select(i =>
                connection.PublishAsync(new OutboundMessage<Foo>(new Foo {Bar = i, Buzz = LoremIpsum }), exchange, routingKey)));
        }
    }
}