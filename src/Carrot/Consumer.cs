using System;
using System.Threading.Tasks;

namespace Carrot
{
    public abstract class Consumer<TMessage> : IConsumer
        where TMessage : class
    {
        public abstract Task ConsumeAsync(ConsumingContext<TMessage> context);

        Task IConsumer.ConsumeAsync(ConsumingContext context)
        {
            return ConsumeAsync(context.To<TMessage>());
        }

        public virtual void OnError(Exception exception) { }

        public virtual void OnConsumeCompletion() { }
    }
}