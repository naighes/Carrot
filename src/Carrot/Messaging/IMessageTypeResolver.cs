using System;

namespace Carrot.Messaging
{
    public interface IMessageTypeResolver
    {
        MessageType Resolve(String source);
    }
}