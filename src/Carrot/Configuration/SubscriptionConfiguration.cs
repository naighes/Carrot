using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Fallback;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class SubscriptionConfiguration
    {
        private readonly IFallbackStrategy _fallbackStrategy;
        private readonly IDictionary<Type, ISet<IConsumer>> _subscriptions = new Dictionary<Type, ISet<IConsumer>>();

        internal SubscriptionConfiguration()
            : this(NoFallbackStrategy.Instance)
        {
        }

        internal SubscriptionConfiguration(IFallbackStrategy fallbackStrategy)
        {
            _fallbackStrategy = fallbackStrategy;
        }

        internal IFallbackStrategy FallbackStrategy
        {
            get { return _fallbackStrategy; }
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