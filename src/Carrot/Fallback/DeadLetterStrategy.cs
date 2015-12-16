using System;
using Carrot.Messages;

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

        public void Apply(IOutboundChannel channel, ConsumedMessageBase message)
        {
            channel.ForwardAsync(message, _exchange, String.Empty);
        }
    }
}