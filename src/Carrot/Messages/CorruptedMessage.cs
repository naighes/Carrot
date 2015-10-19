using System;
using System.Threading.Tasks;
using Carrot.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class CorruptedMessage : ConsumedMessageBase
    {
        internal CorruptedMessage(BasicDeliverEventArgs args)
            : base(args) 
        {
        }

        internal override Object Content
        {
            get { return null; }
        }

        internal override Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration)
        {
            return Task.FromResult((AggregateConsumingResult)new CorruptedMessageConsumingFailure(this));
        }

        internal override Boolean Match(Type type)
        {
            return false;
        }
    }

    internal class CorruptedMessageConsumingFailure : ConsumingFailureBase
    {
        internal CorruptedMessageConsumingFailure(ConsumedMessageBase message)
            : base(message)
        {
        }

        internal override void ReplyAsync(IModel model)
        {
            Message.Acknowledge(model);
        }
    }
}