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
            return New(channel, queue, _ => String.Format("{0}::dle", _));
        }

        public static IFallbackStrategy New(IChannel channel,
                                            Queue queue,
                                            Func<String, String> exchangeNameBuilder)
        {
            var exchangeName = exchangeNameBuilder(queue.Name);
            var exchange = channel.DeclareDurableDirectExchange(exchangeName);
            return new DeadLetterStrategy(exchange);
        }

        public void Apply(IModel model, ConsumedMessageBase message)
        {
            message.ForwardTo(model, _exchange);
        }
    }
}