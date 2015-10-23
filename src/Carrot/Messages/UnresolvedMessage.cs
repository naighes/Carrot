using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class UnresolvedMessage : NonConsumableMessage
    {
        internal UnresolvedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        protected override ConsumingFailureBase Result()
        {
            return new UnresolvedMessageConsumingFailure(this);
        }
    }
}