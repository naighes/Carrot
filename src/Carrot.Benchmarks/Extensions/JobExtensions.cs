using System;
using System.Reflection;
using Carrot.Benchmarks.Jobs;

namespace Carrot.Benchmarks.Extensions
{
    public static class JobExtensions
    {
        public static String Name(this IJob job)
        {
            var attribute = job.GetType().GetTypeInfo().GetCustomAttribute<JobNameAttribute>();

            return attribute == null ? "untitled" : attribute.Name;
        }
    }
}