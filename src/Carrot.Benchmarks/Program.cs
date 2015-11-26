using System;
using System.Linq;
using Carrot.Benchmarks.Jobs;
using Carrot.Configuration;

namespace Carrot.Benchmarks
{
    public class Program
    {
        protected const String RoutingKey = "routing_key";
        private const String EndpointUrl = "amqp://guest:guest@localhost:5672/";

        private static Exchange DeclareExchange(IChannel channel)
        {
            var exchange = channel.DeclareDirectExchange("test_benchmarks_exchange");
            var queue = channel.DeclareQueue("test_benchmarks_queue");
            channel.DeclareExchangeBinding(exchange, queue, RoutingKey);
            return exchange;
        }

        private static Exchange DeclareDurableExchange(IChannel channel)
        {
            var exchange = channel.DeclareDurableDirectExchange("test_benchmarks_durable_exchange");
            var queue = channel.DeclareDurableQueue("test_benchmarks_durable_queue");
            channel.DeclareExchangeBinding(exchange, queue, RoutingKey);
            return exchange;
        }

        private static void Main()
        {
            var channel = BuildChannel();
            var exchange = DeclareExchange(channel);
            var durableExchange = DeclareDurableExchange(channel);

            const Int32 count = 100000;
            const Int32 times = 3;

            var jobs = Enumerable.Repeat(new DurableMessagesPublishJob(channel, durableExchange, RoutingKey),
                                         times)
                                 .Cast<IJob>()
                                 .Concat(Enumerable.Repeat(new NonDurableMessagesPublishJob(channel, exchange, RoutingKey),
                                                           times))
                                 .ToArray();

            foreach (var job in jobs)
            {
                GC.Collect();
                GC.Collect();
                job.RunAsync(count).Result.Print(Console.Out);
            }
            
            Console.ReadLine();
        }

        private static IChannel BuildChannel()
        {
            return Channel.New(_ =>
                               {
                                   _.Endpoint(new Uri(EndpointUrl, UriKind.Absolute));
                                   _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).Assembly));
                               });
        }
    }
}