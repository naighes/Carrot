using Carrot.Messages;

namespace Carrot.Configuration
{
    public interface IMessageTypeResolver
    {
        MessageBinding Resolve(ConsumedMessageContext context);

        MessageBinding Resolve<TMessage>(TMessage message) where TMessage : class;
    }
}