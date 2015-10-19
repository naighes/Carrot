using System;
using System.Threading.Tasks;
using Carrot.Messaging;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class CorruptedMessage : ConsumedMessageBase
    {
        internal CorruptedMessage(HeaderCollection headers, BasicDeliverEventArgs args)
            : base(headers, args) 
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