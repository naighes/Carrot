using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public interface IPublishResult { }

    public class FailurePublishing : IPublishResult
    {
        public readonly Exception Exception;

        internal FailurePublishing(Exception exception)
        {
            Exception = exception;
        }
    }

    public class SuccessfulPublishing : IPublishResult
    {
        public readonly String MessageId;
        public readonly Int64 Timestamp;

        private SuccessfulPublishing(String messageId, Int64 timestamp)
        {
            MessageId = messageId;
            Timestamp = timestamp;
        }

        internal static SuccessfulPublishing FromBasicProperties(IBasicProperties properties)
        {
            return new SuccessfulPublishing(properties.MessageId, properties.Timestamp.UnixTime);
        }
    }

    public abstract class AggregateConsumingResult
    {
        public abstract void Reply(IModel model);
    }

    public class Success : AggregateConsumingResult
    {
        private readonly ConsumedMessageBase _message;

        internal Success(ConsumedMessageBase message)
        {
            _message = message;
        }

        public override void Reply(IModel model)
        {
            _message.Acknowledge(model);
        }
    }

    public class ReiteratedConsumingFailure : ConsumingFailureBase
    {
        internal ReiteratedConsumingFailure(ConsumedMessageBase message, params Exception[] exceptions)
            : base(message, exceptions)
        {
        }

        public override void Reply(IModel model)
        {
            Message.Acknowledge(model);
        }
    }

    public class ConsumingFailure : ConsumingFailureBase
    {
        internal ConsumingFailure(ConsumedMessageBase message, params Exception[] exceptions)
            : base(message, exceptions)
        {
        }

        public override void Reply(IModel model)
        {
            Message.Requeue(model);
        }
    }

    public abstract class ConsumingFailureBase : AggregateConsumingResult
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

    internal class UnsupportedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnsupportedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }

        public override void Reply(IModel model)
        {
            Message.Acknowledge(model);
        }
    }

    internal class UnresolvedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnresolvedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }

        public override void Reply(IModel model)
        {
            Message.Acknowledge(model);
        }
    }

    internal class CorruptedMessageConsumingFailure : ConsumingFailureBase
    {
        internal CorruptedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }

        public override void Reply(IModel model)
        {
            Message.Acknowledge(model);
        }
    }
}