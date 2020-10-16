using System;
using System.Threading.Tasks;
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
                                            Func<string, string> exchangeNameBuilder)
        {
            return new DeadLetterStrategy(broker.DeclareDurableDirectExchange(exchangeNameBuilder(queue.Name)));
        }

        public async Task<IFallbackApplied> Apply(IOutboundChannel channel, ConsumedMessageBase message)
        {
            var published = await channel.ForwardAsync(message, _exchange, string.Empty);
            
            if (published is FailurePublishing result)
                return new FallbackAppliedFailure(result.Exception);

            return new FallbackAppliedSuccessful();
        }
    }
}