using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class SubscriptionConfiguration
    {
        private readonly IDictionary<Type, IConsumer> _subscriptions = new Dictionary<Type, IConsumer>();

        public void Consumes<TMessage>(Consumer<TMessage> consumer) where TMessage : class
        {
            _subscriptions.Add(typeof(TMessage), consumer);
        }

        internal IEnumerable<IConsumer> FindSubscriptions(ConsumedMessageBase message)
        {
            return _subscriptions.Where(_ => message.Match(_.Key))
                                 .Select(_ => _.Value);
        }
    }
}