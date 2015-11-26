using System;

namespace Carrot.Benchmarks
{
    public class JobNameAttribute : Attribute
    {
        public JobNameAttribute(String name)
        {
            Name = name;
        }

        public String Name { get; }
    }
}