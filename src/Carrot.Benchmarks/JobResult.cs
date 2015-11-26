using System;
using System.IO;
using Carrot.Benchmarks.Extensions;
using Carrot.Benchmarks.Jobs;

namespace Carrot.Benchmarks
{
    public struct JobResult
    {
        public readonly String Name;
        public readonly Double Seconds;
        public readonly Int32 Count;

        private JobResult(String name, Double seconds, Int32 count)
        {
            Name = name;
            Seconds = seconds;
            Count = count;
        }

        public static JobResult New(TimeSpan elapsed, IJob job, Int32 count)
        {
            var seconds = elapsed.TotalSeconds;
            return new JobResult(job.Name(), seconds, count);
        }

        public void Print(TextWriter writer)
        {
            Console.WriteLine("[{0}] processed {1} entries in {2} seconds ({3}/s)",
                              Name,
                              Count,
                              Seconds,
                              (Int32)(Count / Seconds));
        }
    }
}