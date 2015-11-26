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

        private static Exchange DeclareExchange(IChannel channel)
        {
            var exchange = channel.DeclareDirectExchange("test_benchmarks_exchange");
            var queue = channel.DeclareQueue("test_benchmarks_queue");
            channel.TryDeclareExchangeBinding(exchange, queue, RoutingKey);
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
            var writer = BuildWriter();
            var channels = new Dictionary<String, IChannel>
                               {
                                   { "default", BuildChannel() },
                                   { "reliable", BuildReliableChannel() }
                               };

            foreach (var pair in channels)
            {
                writer.WriteLine("using '{0}' channel...", pair.Key);
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

        private static void RunOn(IChannel channel, Int32 times, Int32 count, TextWriter writer)
        {
            var exchange = DeclareExchange(channel);
            var durableExchange = DeclareDurableExchange(channel);

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
                job.RunAsync(count).Result.Print(writer);
            }
        }

        private static IChannel BuildChannel()
        {
            return Channel.New(_ =>
                               {
                                   _.Endpoint(new Uri(EndpointUrl, UriKind.Absolute));
                                   _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).Assembly));
                               });
        }

        private static IChannel BuildReliableChannel()
        {
            return Channel.New(_ =>
                               {
                                   _.Endpoint(new Uri(EndpointUrl, UriKind.Absolute));
                                   _.ResolveMessageTypeBy(new MessageBindingResolver(typeof(Foo).Assembly));
                                   _.PublishBy(OutboundChannel.Reliable);
                               });
        }
    }
}