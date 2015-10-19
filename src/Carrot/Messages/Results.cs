using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    internal abstract class AggregateConsumingResult
    {
        internal abstract void Reply(IModel model);
    }

    internal class Success : AggregateConsumingResult
    {
        private readonly ConsumedMessageBase _message;

        internal Success(ConsumedMessageBase message)
        {
            _message = message;
        }

        internal override void Reply(IModel model)
        {
            _message.Acknowledge(model);
        }
    }

    internal class ReiteratedConsumingFailure : ConsumingFailureBase
    {
        internal ReiteratedConsumingFailure(ConsumedMessageBase message, params Exception[] exceptions)
            : base(message, exceptions)
        {
        }

        internal override void Reply(IModel model)
        {
            Message.Acknowledge(model);
        }
    }

    internal class ConsumingFailure : ConsumingFailureBase
    {
        internal ConsumingFailure(ConsumedMessageBase message, params Exception[] exceptions)
            : base(message, exceptions)
        {
        }

        internal override void Reply(IModel model)
        {
            Message.Requeue(model);
        }
    }

    internal abstract class ConsumingFailureBase : AggregateConsumingResult
    {
        protected readonly ConsumedMessageBase Message;
        private readonly Exception[] _exceptions;

        protected ConsumingFailureBase(ConsumedMessageBase message, params Exception[] exceptions)
        {
            Message = message;
            _exceptions = exceptions;
        }

        internal Exception[] Exceptions
        {
            get { return _exceptions ?? new Exception[] { }; }
        }
    }
}