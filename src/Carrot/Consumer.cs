using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot
{
    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        public abstract Task ConsumeAsync(ConsumedMessage<TMessage> message);

        Task IConsumer.ConsumeAsync(ConsumedMessage message)
        {
            return ConsumeAsync(message.As<TMessage>());
        }

        public virtual void OnError(Exception exception) { }
    }
}