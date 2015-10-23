using System;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    #region publishing

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

    #endregion

    public abstract class AggregateConsumingResult
    {
        protected readonly ConsumedMessageBase Message;

        protected AggregateConsumingResult(ConsumedMessageBase message)
        {
            Message = message;
        }

        internal virtual AggregateConsumingResult Reply(IModel model)
        {
            Message.Acknowledge(model);
            return this;
        }
    }

    public class Success : AggregateConsumingResult
    {
        internal Success(ConsumedMessageBase message)
            : base(message)
        {
        }
    }

    public abstract class ConsumingFailureBase : AggregateConsumingResult
    {
        private readonly Exception[] _exceptions;

        protected ConsumingFailureBase(ConsumedMessageBase message, params Exception[] exceptions)
            : base(message)
        {
            _exceptions = exceptions;
        }

        internal Exception[] Exceptions
        {
            get { return _exceptions ?? new Exception[] { }; }
        }
    }

    public class ReiteratedConsumingFailure : ConsumingFailureBase
    {
        internal ReiteratedConsumingFailure(ConsumedMessageBase message,
                                            params Exception[] exceptions)
            : base(message, exceptions)
        {
        }
    }

    public class ConsumingFailure : ConsumingFailureBase
    {
        internal ConsumingFailure(ConsumedMessageBase message, params Exception[] exceptions)
            : base(message, exceptions)
        {
        }

        internal override AggregateConsumingResult Reply(IModel model)
        {
            Message.Requeue(model);
            return this;
        }
    }

    internal class UnsupportedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnsupportedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }
    }

    internal class UnresolvedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnresolvedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }
    }

    internal class CorruptedMessageConsumingFailure : ConsumingFailureBase
    {
        internal CorruptedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }
    }
}