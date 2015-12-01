using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Extensions;
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

        internal override Task<AggregateConsumingResult> ConsumeAsync(IEnumerable<IConsumer> subscriptions)
        {
            return Task.WhenAll(subscriptions.Select(_ => _.SafeConsumeAsync(this)))
                       .ContinueWith(AggregateResult);
        }

        private AggregateConsumingResult BuildErrorResult(IEnumerable<ConsumingResult> results)
        {
            var exceptions = results.OfType<Failure>()
                                    .Select(_ => _.Exception)
                                    .ToArray();

            if (Args.Redelivered)
                return new ReiteratedConsumingFailure(this, exceptions);

            return new ConsumingFailure(this, exceptions);
        }

        private AggregateConsumingResult AggregateResult(Task<ConsumingResult[]> task)
        {
            return task.Result.OfType<Failure>().Any()
                    ? BuildErrorResult(task.Result)
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