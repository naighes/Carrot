using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Messaging
{
    public interface IConsumer
    {
        Task Consume(ConsumedMessage message);

        void OnError(Exception exception);
    }
}