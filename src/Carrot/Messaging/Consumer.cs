using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Messaging
{
    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        public abstract Task Consume(TMessage message);

        Task IConsumer.Consume(ConsumedMessageBase message)
        {
            // TODO: check is proper type.
            return Consume(message.Content as TMessage);
        }
    }
}