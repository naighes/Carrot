using System;
using System.Linq;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    internal abstract class AggregateConsumingResult
    {
        internal abstract void ReplyAsync(IModel model);
    }

    internal class Success : AggregateConsumingResult
    {
        private readonly ConsumedMessageBase _message;

        public Success(ConsumedMessageBase message)
        {
            _message = message;
        }

        internal override void ReplyAsync(IModel model)
        {
            _message.Ack(model);
        }
    }

    internal class Failure : AggregateConsumingResult
    {
        private readonly ConsumedMessageBase _message;
        private readonly Exception[] _exceptions;

        protected Failure(ConsumedMessageBase message, params Exception[] exceptions)
        {
            _message = message;
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

        internal override void ReplyAsync(IModel model)
        {
            _message.Ack(model);
        }
    }
}