using System;
using System.Linq;

namespace Carrot.Messages
{
    public abstract class AggregateConsumingResult
    {
    }

    public class Success : AggregateConsumingResult
    {
        private readonly ConsumedMessageBase _message;

        public Success(ConsumedMessageBase message)
        {
            _message = message;
        }
    }

    public class Failure : AggregateConsumingResult
    {
        private readonly Exception[] _exceptions;

        protected Failure(ConsumedMessageBase message, params Exception[] exceptions)
        {
            _exceptions = exceptions;
        }

        public Exception[] Exceptions
        {
            get { return _exceptions ?? new Exception[] { }; }
        }

        internal static AggregateConsumingResult Build(ConsumedMessageBase message,
                                                        ConsumedMessage.ConsumingResult[] results)
        {
            return new Failure(message,
                               results.OfType<ConsumedMessage.Failure>()
                                      .Select(_ => _.Exception)
                                      .ToArray());
        }
    }
}