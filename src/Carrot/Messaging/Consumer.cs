using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Messaging
{
    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        public abstract Task Consume(Message<TMessage> message);

        Task IConsumer.Consume(ConsumedMessage message)
        {
            return Consume(message.As<TMessage>());
        }

        public virtual void OnError(Exception exception) { }
    }
}