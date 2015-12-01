using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Fallback;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    // TODO: should be made sealed.
    public class ConsumedMessage : ConsumedMessageBase
    {
        internal ConsumedMessage(Object content, BasicDeliverEventArgs args)
            : base(args)
        {
            Content = content;
        }

        internal override Object Content { get; }

        internal override Boolean Match(Type type)
        {
            return Content != null && type.IsInstanceOfType(Content);
        }

        internal override Task<AggregateConsumingResult> ConsumeAsync(ConsumingConfiguration configuration)
        {
            return Task.WhenAll(configuration.FindSubscriptions(this)
                                             .Select(_ => _.SafeConsumeAsync(this)))
                       .ContinueWith(_ => AggregateResult(_, configuration.FallbackStrategy));
        }

        private AggregateConsumingResult BuildErrorResult(IEnumerable<ConsumingResult> results,
                                                          IFallbackStrategy fallbackStrategy)
        {
            var exceptions = results.OfType<Failure>()
                                    .Select(_ => _.Exception)
                                    .ToArray();

            if (Args.Redelivered)
                return new ReiteratedConsumingFailure(this, fallbackStrategy, exceptions);

            return new ConsumingFailure(this, fallbackStrategy, exceptions);
        }

        private AggregateConsumingResult AggregateResult(Task<ConsumingResult[]> task, IFallbackStrategy fallbackStrategy)
        {
            return task.Result.OfType<Failure>().Any()
                    ? BuildErrorResult(task.Result, fallbackStrategy)
                    : new Messages.Success(this);
        }

        internal abstract class ConsumingResult
        {
            protected readonly ConsumedMessageBase Message;

            protected ConsumingResult(ConsumedMessageBase message)
            {
                Message = message;
            }
        }

        internal class Failure : ConsumingResult
        {
            internal readonly Exception Exception;

            internal Failure(ConsumedMessageBase message, Exception exception)
                : base(message)
            {
                Exception = exception;
            }
        }

        internal class Success : ConsumingResult
        {
            public Success(ConsumedMessageBase message)
                : base(message)
            {
            }
        }
    }
}