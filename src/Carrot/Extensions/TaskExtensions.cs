using System;
using System.Threading.Tasks;

namespace Carrot.Extensions
{
    public static class TaskExtensions
    {
        public static Task<TResult> StartNew<TInput, TResult>(this TaskFactory<TResult> factory,
                                                              Func<TInput, TResult> function, 
                                                              TInput state)
        {
            return factory.StartNew(_ => function((TInput)_), state);
        }
    }
}