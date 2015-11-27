using System;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot.Fallback
{
    public class DeadLetterStrategy : IFallbackStrategy
    {
        private readonly Exchange _exchange;

        private DeadLetterStrategy(Exchange exchange)
        {
            _exchange = exchange;
        }

        public static IFallbackStrategy New(IBroker broker, Queue queue)
        {
            return New(broker, queue, _ => $"{_}::dle");
        }

        public static IFallbackStrategy New(IBroker broker,
                                            Queue queue,
                                            Func<String, String> exchangeNameBuilder)
        {
            return new DeadLetterStrategy(broker.DeclareDurableDirectExchange(exchangeNameBuilder(queue.Name)));
        }

        public void Apply(IModel model, ConsumedMessageBase message)
        {
            message.PersistentForwardTo(model, _exchange, String.Empty, true, false);
        }
    }
}