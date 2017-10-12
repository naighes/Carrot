using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            return Content != null && type.GetTypeInfo().IsInstanceOfType(Content);
        }

        internal override Task<AggregateConsumingResult> ConsumeAsync(IEnumerable<IConsumer> subscriptions)
        {
            return Task.WhenAll(subscriptions.Select(_ => _.SafeConsumeAsync(this)))
                       .ContinueWith(AggregateResult);
        }

        private AggregateConsumingResult BuildErrorResult(IEnumerable<ConsumingResult> results)
        {
            var consumingResults = results.ToArray();
            var exceptions = consumingResults.OfType<Failure>()
                                    .Select(_ => _.Exception)
                                    .ToArray();

            if (Args.Redelivered)
                return new ReiteratedConsumingFailure(this, consumingResults, exceptions);

            return new ConsumingFailure(this, consumingResults, exceptions);
        }

        private AggregateConsumingResult AggregateResult(Task<ConsumingResult[]> task)
        {
            return task.Result.OfType<Failure>().Any()
                       ? BuildErrorResult(task.Result)
                       : new Messages.Success(this, task.Result);
        }

        public abstract class ConsumingResult
        {
            protected readonly ConsumedMessageBase Message;
            protected readonly IConsumer Consumer;

            protected ConsumingResult(ConsumedMessageBase message, IConsumer consumer)
            {
                Message = message;
                Consumer = consumer;
            }

            internal void NotifyConsumingCompletion()
            {
                Consumer.OnConsumeCompletion();
            }
        }

        internal class Failure : ConsumingResult
        {
            internal readonly Exception Exception;

            internal Failure(ConsumedMessageBase message, IConsumer consumer, Exception exception)
                : base(message, consumer)
            {
                Exception = exception;
            }
        }

        internal class Success : ConsumingResult
        {
            public Success(ConsumedMessageBase message, IConsumer consumer)
                : base(message, consumer)
            {
            }
        }
    }
}