using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Messages
{
    public class UnresolvedMessage : ConsumedMessageBase
    {
        public UnresolvedMessage(HeaderCollection headers, 
                                 UInt64 deliveryTag,
                                 Boolean redelivered)
            : base(headers, deliveryTag, redelivered)
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        internal override Task<IAggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult((IAggregateConsumingResult)new UnresolvedMessageFailure());
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }

    public class UnresolvedMessageFailure : Failure
    {
        internal UnresolvedMessageFailure()
        {
        }
    }
}