using Carrot.Fallback;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class UnsupportedMessage : NonConsumableMessage
    {
        internal UnsupportedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        protected override ConsumingFailureBase Result(IFallbackStrategy fallbackStrategy)
        {
            return new UnsupportedMessageConsumingFailure(this, fallbackStrategy);
        }
    }
}