using System;
using System.Linq;
using Carrot.Extensions;
using Carrot.Fallback;
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
        private readonly IFallbackStrategy _fallbackStrategy;
        private readonly Exception[] _exceptions;

        protected ConsumingFailureBase(ConsumedMessageBase message,
                                       IFallbackStrategy fallbackStrategy,
                                       params Exception[] exceptions)
            : base(message)
        {
            _fallbackStrategy = fallbackStrategy;
            _exceptions = exceptions;
        }

        internal Exception[] Exceptions
        {
            get { return _exceptions ?? new Exception[] { }; }
        }

        internal override AggregateConsumingResult Reply(IModel model)
        {
            _fallbackStrategy.Apply(model, Message);
            return base.Reply(model);
        }

        internal void WithErrors(Action<Exception> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            Exceptions.NotNull()
                      .ToList()
                      .ForEach(action);
        }
    }

    public class ReiteratedConsumingFailure : ConsumingFailureBase
    {
        internal ReiteratedConsumingFailure(ConsumedMessageBase message,
                                            IFallbackStrategy fallbackStrategy,
                                            params Exception[] exceptions)
            : base(message, fallbackStrategy, exceptions)
        {
        }
    }

    public class ConsumingFailure : ConsumingFailureBase
    {
        internal ConsumingFailure(ConsumedMessageBase message,
                                  IFallbackStrategy fallbackStrategy,
                                  params Exception[] exceptions)
            : base(message, fallbackStrategy, exceptions)
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
        internal UnsupportedMessageConsumingFailure(ConsumedMessageBase message, IFallbackStrategy fallbackStrategy)
            : base(message, fallbackStrategy)
        {
        }
    }

    internal class UnresolvedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnresolvedMessageConsumingFailure(ConsumedMessageBase message, IFallbackStrategy fallbackStrategy)
            : base(message, fallbackStrategy)
        {
        }
    }

    internal class CorruptedMessageConsumingFailure : ConsumingFailureBase
    {
        internal CorruptedMessageConsumingFailure(ConsumedMessageBase message, IFallbackStrategy fallbackStrategy)
            : base(message, fallbackStrategy)
        {
        }
    }
}