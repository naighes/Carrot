using System;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Messaging;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class ConsumedMessage : ConsumedMessageBase
    {
        private readonly Object _content;

        internal ConsumedMessage(Object content, BasicDeliverEventArgs args)
            : base(args)
        {
            _content = content;
        }

        internal override Object Content
        {
            get { return _content; }
        }

        private static AggregateConsumingResult AggregateResult(Task<ConsumingResult[]> task, ConsumedMessageBase message)
        {
            // TODO
            return task.Result.OfType<Failure>().Any()
                    ? Messages.Failure.Build(message, task.Result)
                    : new Messages.Success(message);
        }

        internal override Boolean Match(Type type)
        {
            return Content != null && type.IsInstanceOfType(Content);
        }

        internal override Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.WhenAll(configuration.FindSubscriptions(this)
                                             .Select(_ => new OuterConsumer(_.Value).ConsumeAsync(this)))
                       .ContinueWith(_ => AggregateResult(_, this));
        }

        internal abstract class ConsumingResult
        {
            protected readonly ConsumedMessage Message;

            protected ConsumingResult(ConsumedMessage message)
            {
                Message = message;
            }
        }

        internal class Failure : ConsumingResult
        {
            internal readonly Exception Exception;

            internal Failure(ConsumedMessage message, Exception exception)
                : base(message)
            {
                Exception = exception;
            }
        }

        internal class Success : ConsumingResult
        {
            public Success(ConsumedMessage message)
                : base(message)
            {
            }
        }
    }
}