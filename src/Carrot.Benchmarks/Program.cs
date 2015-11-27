using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Carrot.Benchmarks.Jobs;
using Carrot.Configuration;

namespace Carrot.Benchmarks
{
    public class Program
    {
        protected const String RoutingKey = "routing_key";
        private const String EndpointUrl = "amqp://guest:guest@localhost:5672/";
        private const Int32 Count = 100000;
        private const Int32 Times = 3;

        private static Exchange DeclareExchange(IBroker broker)
        {
            var exchange = broker.DeclareDirectExchange("test_benchmarks_exchange");
            var queue = broker.DeclareQueue("test_benchmarks_queue");
            broker.TryDeclareExchangeBinding(exchange, queue, RoutingKey);
            return exchange;
        }

        private static Exchange DeclareDurableExchange(IBroker broker)
        {
            var exchange = broker.DeclareDurableDirectExchange("test_benchmarks_durable_exchange");
            var queue = broker.DeclareDurableQueue("test_benchmarks_durable_queue");
            broker.DeclareExchangeBinding(exchange, queue, RoutingKey);
            return exchange;
        }

        private static void Main()
        {
            var writer = BuildWriter();
            var brokers = new Dictionary<String, IBroker>
                              {
                                  { "default", BuildBroker() },
                                  { "reliable", BuildReliableBroker() }
                              };

            foreach (var pair in brokers)
            {
                writer.WriteLine("using '{0}' broker...", pair.Key);
                writer.WriteLine("----------------------------------------------------------");
                RunOn(pair.Value, Times, Count, writer);
                writer.WriteLine();
            }

            Console.ReadLine();
            writer.Dispose();
        }

        private static BatchWriter BuildWriter()
        {
            const Int32 bufferSize = 4096;
            var path = Path.Combine(Environment.CurrentDirectory,
                                    String.Concat(String.Format(DateTime.UtcNow.ToString("yyyymmddHHmm",
                                                                                         CultureInfo.InvariantCulture)),
                                                  ".log"));
            var stream = new FileStream(path,
                                        FileMode.Append,
                                        FileAccess.Write,
                                        FileShare.Read,
                                        bufferSize);
            return new BatchWriter(Console.Out,
                                   new StreamWriter(stream, new UTF8Encoding(true), bufferSize, false));
        }

        private static void RunOn(IBroker broker, Int32 times, Int32 count, TextWriter writer)
        {
            var exchange = DeclareExchange(broker);
            var durableExchange = DeclareDurableExchange(broker);

            var jobs = Enumerable.Repeat(new DurableMessagesPublishJob(broker, durableExchange, RoutingKey),
                                         times)
                                 .Cast<IJob>()
                                 .Concat(Enumerable.Repeat(new NonDurableMessagesPublishJob(broker, exchange, RoutingKey),
                                                           times))
                                 .ToArray();

            foreach (var job in jobs)
            {
                GC.Collect();
                GC.Collect();
                job.RunAsync(count).Result.Print(writer);
            }
        }

        private static IBroker BuildBroker()
        {
            return Broker.New(_ =>
                               {
                                   _.Endpoint(new Uri(EndpointUrl, UriKind.Absolute));
                                   _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).Assembly));
                               });
        }

        private static IBroker BuildReliableBroker()
        {
            return Broker.New(_ =>
                               {
                                   _.Endpoint(new Uri(EndpointUrl, UriKind.Absolute));
                                   _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).Assembly));
                                   _.PublishBy(OutboundChannel.Reliable);
                               });
        }
    }
}