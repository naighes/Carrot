using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Messages;

namespace Carrot.Messaging
{
    public class SubscriptionConfiguration
    {
        private readonly IDictionary<Type, IConsumer> _subscriptions = new Dictionary<Type, IConsumer>();

        public void Consumes<TMessage>(Consumer<TMessage> consumer) where TMessage : class
        {
            _subscriptions.Add(typeof(TMessage), consumer);
        }

        internal IDictionary<Type, IConsumer> FindSubscriptions(ConsumedMessageBase message)
        {
            return _subscriptions.Where(_ => message.Match(_.Key))
                                 .ToDictionary(_ => _.Key, _ => _.Value);
        }
    }
}