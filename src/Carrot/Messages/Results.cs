using System;
using System.Linq;

namespace Carrot.Messages
{
    public interface IAggregateConsumingResult
    {
    }

    public class Success : IAggregateConsumingResult
    {
    }

    public class Failure : IAggregateConsumingResult
    {
        private readonly Exception[] _exceptions;

        protected Failure(params Exception[] exceptions)
        {
            _exceptions = exceptions;
        }

        public Exception[] Exceptions
        {
            get { return _exceptions ?? new Exception[] { }; }
        }

        internal static IAggregateConsumingResult Build(ConsumedMessage.IConsumingResult[] results)
        {
            return new Failure(results.OfType<ConsumedMessage.Failure>()
                                      .Select(_ => _.Exception)
                                      .ToArray());
        }
    }
}