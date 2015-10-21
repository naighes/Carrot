using System;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public abstract class ConsumedMessageBase
    {
        protected readonly HeaderCollection Headers;
        protected readonly UInt64 DeliveryTag;
        protected readonly Boolean Redelivered;

        protected ConsumedMessageBase(BasicDeliverEventArgs args)
        {
            Headers = HeaderCollection.Parse(args);
            DeliveryTag = args.DeliveryTag;
            Redelivered = args.Redelivered;
        }

        internal abstract Object Content { get; }

        internal abstract Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration);

        internal ConsumedMessage<TMessage> As<TMessage>() where TMessage : class
        {
            var content = Content as TMessage;

            if (content == null)
                throw new InvalidCastException(String.Format("cannot cast '{0}' to '{1}'",
                                                             Content.GetType(),
                                                             typeof(TMessage)));

            return new ConsumedMessage<TMessage>(content, Headers);
        }

        internal abstract Boolean Match(Type type);

        internal void Acknowledge(IModel model)
        {
            model.BasicAck(DeliveryTag, false);
        }

        internal void Requeue(IModel model)
        {
            model.BasicNack(DeliveryTag, false, true);
        }

        internal AggregateConsumingResult BuildErrorResult(ConsumedMessage.ConsumingResult[] results)
        {
            var exceptions = results.OfType<ConsumedMessage.Failure>()
                                    .Select(_ => _.Exception)
                                    .ToArray();

            if (Redelivered)
                return new ReiteratedConsumingFailure(this, exceptions);

            return new ConsumingFailure(this, exceptions);
        }
    }
}