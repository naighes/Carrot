using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class MessageQueue : IEquatable<MessageQueue>
    {
        private readonly String _name;
        private readonly IConsumedMessageBuilder _builder;

        private readonly ISet<ConsumingPromise> _promises = new HashSet<ConsumingPromise>();

        private MessageQueue(String name, IConsumedMessageBuilder builder)
        {
            _name = name;
            _builder = builder;
        }

        internal String Name
        {
            get { return _name; }
        }

        public static Boolean operator ==(MessageQueue left, MessageQueue right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(MessageQueue left, MessageQueue right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(MessageQueue other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return String.Equals(_name, other._name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MessageQueue;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return _name.GetHashCode();
        }

        public void SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure)
        {
            Subscribe(configure,
                      (b, c) => new AtMostOnceConsumingPromise(this, b, c));
        }

        public void SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure)
        {
            Subscribe(configure,
                      (b, c) => new AtLeastOnceConsumingPromise(this, b, c));
        }

        internal static MessageQueue New(String name, IConsumedMessageBuilder builder)
        {
            return new MessageQueue(name, builder);
        }

        internal void Declare(IModel model)
        {
            model.QueueDeclare(Name, true, false, false, new Dictionary<String, Object>());

            foreach (var promise in _promises)
                promise.Declare(model);
        }

        private void Subscribe(Action<SubscriptionConfiguration> configure,
                               Func<IConsumedMessageBuilder, SubscriptionConfiguration, ConsumingPromise> func)
        {
            var configuration = new SubscriptionConfiguration();
            configure(configuration);
            _promises.Add(func(_builder, configuration));
        }
    }
}