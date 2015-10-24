using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Fallback;
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
            return Task.FromResult<AggregateConsumingResult>(Result(configuration.FallbackStrategy));
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }

        protected abstract ConsumingFailureBase Result(IFallbackStrategy fallbackStrategy);
    }
}