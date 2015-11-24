using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Fallback;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class ConsumingConfiguration
    {
        private readonly IChannel _channel;
        private readonly Queue _queue;
        private readonly IDictionary<Type, ISet<IConsumer>> _subscriptions = new Dictionary<Type, ISet<IConsumer>>();

        private IFallbackStrategy _fallbackStrategy = NoFallbackStrategy.Instance;

        internal ConsumingConfiguration(IChannel channel, Queue queue)
        {
            _channel = channel;
            _queue = queue;
        }

        internal IFallbackStrategy FallbackStrategy => _fallbackStrategy;

        public void FallbackBy(Func<IChannel, Queue, IFallbackStrategy> strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            _fallbackStrategy = strategy(_channel, _queue);
        }

        public void Consumes<TMessage>(Consumer<TMessage> consumer) where TMessage : class
        {
            var type = typeof(TMessage);

            if (!_subscriptions.ContainsKey(type))
                _subscriptions.Add(type, new HashSet<IConsumer>());

            _subscriptions[type].Add(consumer);
        }

        internal IEnumerable<IConsumer> FindSubscriptions(ConsumedMessageBase message)
        {
            return _subscriptions.Where(_ => message.Match(_.Key))
                                 .SelectMany(_ => _.Value);
        }
    }
}