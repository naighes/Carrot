using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public sealed class CorruptedMessage : NonConsumableMessage
    {
        internal CorruptedMessage(BasicDeliverEventArgs args)
            : base(args) 
        {
        }

        protected override ConsumingFailureBase Result()
        {
            return new CorruptedMessageConsumingFailure(this);
        }
    }
}