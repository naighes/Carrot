using System;

namespace Carrot.Configuration
{
    public interface IMessageTypeResolver
    {
        MessageType Resolve(String source);

        MessageType Resolve<TMessage>() where TMessage : class;
    }
}