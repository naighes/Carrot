using System;

namespace Carrot.Configuration
{
    public interface IMessageTypeResolver
    {
        MessageBinding Resolve(String source);

        MessageBinding Resolve<TMessage>() where TMessage : class;
    }
}