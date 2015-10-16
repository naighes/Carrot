namespace Carrot.Messaging
{
    using System;
    using System.Threading.Tasks;

    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        public abstract Task Consume(TMessage message);

        Task IConsumer.Consume(Object message)
        {
            // TODO: check is proper type.
            return this.Consume(message as TMessage);
        }
    }
}