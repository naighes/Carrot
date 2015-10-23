using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public abstract class ErrorAffectedConsumedMessage : ConsumedMessageBase
    {
        protected ErrorAffectedConsumedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        protected abstract ConsumingFailureBase Result { get; }

        internal override Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult<AggregateConsumingResult>(Result);
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }
}