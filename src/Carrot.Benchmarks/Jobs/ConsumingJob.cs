using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Carrot.Benchmarks.Jobs
{
    [JobName("consuming-messages")]
    public class ConsumingJob : IJob
    {
        protected readonly IBroker Broker;

        private readonly Queue _queue;
        private readonly Stopwatch _stopwatch;

        public ConsumingJob(IBroker broker, Queue queue)
        {
            Broker = broker;
            _queue = queue;
            _stopwatch = new Stopwatch();
        }

        public Task<JobResult> RunAsync(Int32 count)
        {
            var @event = new ManualResetEvent(false);
            Broker.SubscribeByAtLeastOnce(_queue, _ => { _.Consumes(new FooConsumer(count, @event)); });
            var connection = Broker.Connect();
            _stopwatch.Start();
            @event.WaitOne();

            var elapsed = _stopwatch.Elapsed;
            _stopwatch.Stop();
            _stopwatch.Reset();
            connection.Dispose();
            return Task.FromResult(JobResult.New(elapsed, this, count));
        }

        private class FooConsumer : Consumer<Foo>
        {
            private readonly Int32 _expectedCount;
            private readonly ManualResetEvent _event;
            private Int32 _accumulator;

            public FooConsumer(Int32 expectedCount, ManualResetEvent @event)
            {
                _expectedCount = expectedCount;
                _event = @event;
            }

            public override Task ConsumeAsync(ConsumingContext<Foo> context)
            {
                return Task.FromResult(0);
            }

            public override void OnConsumeCompletion()
            {
                base.OnConsumeCompletion();

                var value = Interlocked.Increment(ref _accumulator);

                if (value == _expectedCount)
                    _event.Set();
            }
        }
    }
}