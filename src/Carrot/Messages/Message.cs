using System;

namespace Carrot.Messages
{
    public abstract class Message<TMessage> : IMessage
        where TMessage : class
    {
        public abstract TMessage Content { get; }

        public abstract HeaderCollection Headers { get; }

        public Boolean ContainsHeader(String key) => Headers.ContainsHeader(key);

        Object IMessage.Content => Content;
    }
}
