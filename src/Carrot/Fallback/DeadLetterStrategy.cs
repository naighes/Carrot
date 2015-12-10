using System;
using Carrot.Extensions;
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
            var properties = message.Args.BasicProperties.Copy();
            properties.Persistent = true;
            model.BasicPublish(_exchange.Name,
                               String.Empty,
                               true,
                               false,
                               properties,
                               message.Args.Body);
        }
    }
}