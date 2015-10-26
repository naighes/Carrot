using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public interface IConsumedMessageBuilder
    {
        ConsumedMessageBase Build(BasicDeliverEventArgs args);
    }
}