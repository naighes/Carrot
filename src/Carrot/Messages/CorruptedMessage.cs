using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Messages
{
    public class CorruptedMessage : ConsumedMessageBase
    {
        public CorruptedMessage(HeaderCollection headers, 
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
            return Task.FromResult((IAggregateConsumingResult)new CorruptedMessageFailure());
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }

    public class CorruptedMessageFailure : Failure
    {
        internal CorruptedMessageFailure()
        {
        }
    }
}