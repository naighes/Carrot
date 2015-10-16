using System;
using System.Threading.Tasks;

namespace TowerBridge.Common.Infrastructure.Messaging
{
    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        public abstract Task Consume(TMessage message);

        Task IConsumer.Consume(Object message)
        {
            // TODO: check is proper type.
            return Consume(message as TMessage);
        }
    }
}