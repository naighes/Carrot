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

        internal interface IConsumingResult { }

        internal override Object Content
        {
            get { return _content; }
        }

        private static IAggregateConsumingResult AggregateResult(Task<IConsumingResult[]> task)
        {
            return task.Result.OfType<Failure>().Any()
                    ? Messages.Failure.Build(task.Result)
                    : new Messages.Success();
        }

        internal override Boolean Match(Type type)
        {
            return Content != null && type.IsInstanceOfType(Content);
        }

        internal override Task<IAggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.WhenAll(configuration.FindSubscriptions(this)
                                             .Select(_ => new OuterConsumer(_.Value).ConsumeAsync(this)))
                       .ContinueWith<IAggregateConsumingResult>(AggregateResult);
        }

        internal class Failure : IConsumingResult
        {
            internal readonly Exception Exception;

            internal Failure(Exception exception)
            {
                Exception = exception;
            }
        }

        internal class Success : IConsumingResult { }
    }
}