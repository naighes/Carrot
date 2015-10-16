using System;
using System.Collections.Generic;

namespace TowerBridge.Common.Infrastructure.Messaging
{
    public class SubscriptionConfiguration
    {
        private readonly IDictionary<Type, IConsumer> _subscriptions = new Dictionary<Type, IConsumer>();

        internal IDictionary<Type, IConsumer> Subscriptions
        {
            get { return this._subscriptions; }
        }

        public void Consume<TMessage>(Consumer<TMessage> consumer) where TMessage : class
        {
            _subscriptions.Add(typeof(TMessage), consumer);
        }
    }
}