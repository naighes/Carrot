using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class UnsupportedMessage : ErrorAffectedConsumedMessage
    {
        internal UnsupportedMessage(BasicDeliverEventArgs args)
            : base(args)
        {
        }

        protected override ConsumingFailureBase Result()
        {
            return new UnsupportedMessageConsumingFailure(this);
        }
    }
}