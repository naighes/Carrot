using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class MessageQueue : IEquatable<MessageQueue>
    {
        private readonly Queue _queue;
        private readonly IConsumedMessageBuilder _builder;

        private readonly ISet<ConsumingPromise> _promises = new HashSet<ConsumingPromise>();

        private MessageQueue(Queue queue, IConsumedMessageBuilder builder)
        {
            _queue = queue;
            _builder = builder;
        }

        internal String Queue
        {
            get { return _queue.Name; } // TODO: ugly: it's temporary.
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

            return String.Equals(this._queue, other._queue);
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
            return this._queue.GetHashCode();
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

        internal static MessageQueue New(Queue queue, IConsumedMessageBuilder builder)
        {
            return new MessageQueue(queue, builder);
        }

        internal void Declare(IModel model)
        {
            _queue.Declare(model);

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