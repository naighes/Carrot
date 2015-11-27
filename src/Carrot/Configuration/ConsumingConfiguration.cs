using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Fallback;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class ConsumingConfiguration
    {
        private readonly IBroker _broker;
        private readonly Queue _queue;
        private readonly IDictionary<Type, ISet<IConsumer>> _subscriptions = new Dictionary<Type, ISet<IConsumer>>();

        internal ConsumingConfiguration(IBroker broker, Queue queue)
        {
            _broker = broker;
            _queue = queue;
        }

        internal IFallbackStrategy FallbackStrategy { get; private set; } = NoFallbackStrategy.Instance;

        public void FallbackBy(Func<IBroker, Queue, IFallbackStrategy> strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            FallbackStrategy = strategy(_broker, _queue);
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