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
        protected readonly ConsumedMessage.ConsumingResult[] Results;

        protected AggregateConsumingResult(ConsumedMessageBase message, ConsumedMessage.ConsumingResult[] results)
        {
            Message = message;
            Results = results;
        }

        internal virtual AggregateConsumingResult Reply(IInboundChannel inboundChannel,
                                                        IOutboundChannel outboundChannel,
                                                        IFallbackStrategy fallbackStrategy)
        {
            Message.Acknowledge(inboundChannel);
            return this;
        }

        internal void NotifyConsumingCompletion()
        {
            Results.ToList().ForEach(_ => _.NotifyConsumingCompletion());
        }
    }

    public class Success : AggregateConsumingResult
    {
        internal Success(ConsumedMessageBase message, ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }

    public abstract class ConsumingFailureBase : AggregateConsumingResult
    {
        private readonly Exception[] _exceptions;

        protected ConsumingFailureBase(ConsumedMessageBase message,
                                       ConsumedMessage.ConsumingResult[] results,
                                       params Exception[] exceptions)
            : base(message, results)
        {
            _exceptions = exceptions;
        }

        internal Exception[] Exceptions => _exceptions ?? new Exception[] { };

        internal void WithErrors(Action<Exception> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            Exceptions.NotNull()
                      .ToList()
                      .ForEach(action);
        }
    }

    public class ReiteratedConsumingFailure : ConsumingFailureBase
    {
        internal ReiteratedConsumingFailure(ConsumedMessageBase message,
                                            ConsumedMessage.ConsumingResult[] results,
                                            params Exception[] exceptions)
            : base(message, results, exceptions)
        {
        }

        internal override AggregateConsumingResult Reply(IInboundChannel inboundChannel,
                                                         IOutboundChannel outboundChannel,
                                                         IFallbackStrategy fallbackStrategy)
        {
            fallbackStrategy.Apply(outboundChannel, Message);
            return base.Reply(inboundChannel, outboundChannel, fallbackStrategy);
        }
    }

    public class ConsumingFailure : ConsumingFailureBase
    {
        internal ConsumingFailure(ConsumedMessageBase message,
                                  ConsumedMessage.ConsumingResult[] results,
                                  params Exception[] exceptions)
            : base(message, results, exceptions)
        {
        }

        internal override AggregateConsumingResult Reply(IInboundChannel inboundChannel,
                                                         IOutboundChannel outboundChannel,
                                                         IFallbackStrategy fallbackStrategy)
        {
            Message.Requeue(inboundChannel);
            return this;
        }
    }

    internal class UnsupportedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnsupportedMessageConsumingFailure(ConsumedMessageBase message,
                                                    ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }

    internal class UnresolvedMessageConsumingFailure : ConsumingFailureBase
    {
        internal UnresolvedMessageConsumingFailure(ConsumedMessageBase message,
                                                   ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }

    internal class CorruptedMessageConsumingFailure : ConsumingFailureBase
    {
        internal CorruptedMessageConsumingFailure(ConsumedMessageBase message,
                                                  ConsumedMessage.ConsumingResult[] results)
            : base(message, results)
        {
        }
    }
}