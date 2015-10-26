using Carrot.Fallback;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class UnresolvedMessage : NonConsumableMessage
    {
        internal UnresolvedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        protected override ConsumingFailureBase Result(IFallbackStrategy fallbackStrategy)
        {
            return new UnresolvedMessageConsumingFailure(this, fallbackStrategy);
        }
    }
}