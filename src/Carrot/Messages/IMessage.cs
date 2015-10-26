using System;

namespace Carrot.Messages
{
    public interface IMessage
    {
        Object Content { get; }

        HeaderCollection Headers { get; }
    }
}