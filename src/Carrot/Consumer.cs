using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot
{
    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        public abstract Task ConsumeAsync(ConsumedMessage<TMessage> message);

        Task IConsumer.ConsumeAsync(ConsumedMessageBase message)
        {
            return ConsumeAsync(message.To<TMessage>());
        }

        public virtual void OnError(Exception exception) { }

        public virtual void OnConsumeCompletion() { }
    }
}