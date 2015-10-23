using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class UnresolvedMessage : ConsumedMessageBase
    {
        internal UnresolvedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        internal override Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult<AggregateConsumingResult>(new UnresolvedMessageConsumingFailure(this));
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }
}