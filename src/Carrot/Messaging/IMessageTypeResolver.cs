namespace Carrot.Messaging
{
    using System;

    public interface IMessageTypeResolver
    {
        MessageType Resolve(String source);
    }
}