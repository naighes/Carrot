using System;

namespace Carrot.Messaging
{
    // TODO: fallback strategy for resolver.
    public interface IMessageTypeResolver
    {
        MessageType Resolve(String source);
    }
}