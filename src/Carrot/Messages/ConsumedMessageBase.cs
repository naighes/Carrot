using System;
using System.Threading.Tasks;
using Carrot.Messaging;
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

        internal Message<TMessage> As<TMessage>() where TMessage : class
        {
            var content = Content as TMessage;

            if (content == null)
                throw new InvalidCastException(String.Format("cannot cast '{0}' to '{1}'", 
                                                             Content.GetType(),
                                                             typeof(TMessage)));

            return new Message<TMessage>(content, Headers);
        }

        internal abstract Boolean Match(Type type);
    }
}