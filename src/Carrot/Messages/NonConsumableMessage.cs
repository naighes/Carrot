using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public abstract class NonConsumableMessage : ConsumedMessageBase
    {
        protected NonConsumableMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        internal override Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult<AggregateConsumingResult>(Result());
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }

        protected abstract ConsumingFailureBase Result();
    }
}