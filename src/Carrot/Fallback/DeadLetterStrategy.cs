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

        public void Apply(IOutboundChannelPool channelPool, ConsumedMessageBase message)
        {
            using (var outboundChannel = channelPool.Take())
            {
                outboundChannel.ForwardAsync(message, _exchange, String.Empty);
            }
        }
    }
}