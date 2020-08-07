using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Benchmarks.Jobs
{
    public abstract class PublishJob : IJob
    {
        protected readonly IBroker Broker;

        private readonly String _routingKey;
        private readonly Exchange _exchange;
        private readonly Stopwatch _stopwatch;

        protected PublishJob(IBroker broker, Exchange exchange, String routingKey)
        {
            Broker = broker;
            _exchange = exchange;
            _routingKey = routingKey;
            _stopwatch = new Stopwatch();
        }

        public Task<JobResult> RunAsync(Int32 count)
        {
            var connection = Broker.Connect();
            var tasks = new Task[count];
            _stopwatch.Start();

            for (var i = 0; i < count; i++)
                tasks[i] = connection.PublishAsync(BuildMessage(i), _exchange, _routingKey);

            return Task.WhenAll(tasks)
                       .ContinueWith(_ =>
                                     {
                                         _stopwatch.Stop();
                                     
                                         for (var i = 0; i < count; i++)
                                             tasks[i] = null;
                                     
                                         tasks = null;

                                         var elapsed = _stopwatch.Elapsed;
                                         _stopwatch.Reset();
                                         connection.Dispose();
                                         return JobResult.New(elapsed, this, count);
                                     }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }

        protected abstract OutboundMessage<Foo> BuildMessage(Int32 i);
    }
}