using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot.Fallback
{
    public interface IFallbackStrategy
    {
        void Apply(IModel model, ConsumedMessageBase message);
    }
}