using System;
using System.Threading.Tasks;

namespace Carrot.Benchmarks.Jobs
{
    public interface IJob
    {
        Task<JobResult> RunAsync(Int32 count);
    }
}