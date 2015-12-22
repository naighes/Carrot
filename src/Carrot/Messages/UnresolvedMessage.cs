using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public sealed class UnresolvedMessage : NonConsumableMessage
    {
        internal UnresolvedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        protected override ConsumingFailureBase Result(ConsumedMessage.ConsumingResult[] results)
        {
            return new UnresolvedMessageConsumingFailure(this, results);
        }
    }
}