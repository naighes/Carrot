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

        public static IFallbackStrategy New(IChannel channel, Queue queue)
        {
            return New(channel, queue, _ => $"{_}::dle");
        }

        public static IFallbackStrategy New(IChannel channel,
                                            Queue queue,
                                            Func<String, String> exchangeNameBuilder)
        {
            return new DeadLetterStrategy(channel.DeclareDurableDirectExchange(exchangeNameBuilder(queue.Name)));
        }

        public void Apply(IModel model, ConsumedMessageBase message)
        {
            message.PersistentForwardTo(model, _exchange, String.Empty, true, false);
        }
    }
}