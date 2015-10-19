using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Messages
{
    public class UnsupportedMessage : ConsumedMessageBase
    {
        public UnsupportedMessage(String messageId, 
                                  UInt64 deliveryTag,
                                  Boolean redelivered,
                                  Int64 timestamp)
            : base(messageId, deliveryTag, redelivered, timestamp)
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        internal override Task<IAggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult((IAggregateConsumingResult)new UnsupportedMessageFailure());
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }

    public class UnsupportedMessageFailure : Failure
    {
        internal UnsupportedMessageFailure()
        {
        }
    }
}