using System;

namespace TowerBridge.Common.Infrastructure.Messaging
{
    public interface IMessageTypeResolver
    {
        MessageType Resolve(String source);
    }
}