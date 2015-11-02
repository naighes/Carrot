using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot
{
    public class MessageQueue : IEquatable<MessageQueue>
    {
        private readonly String _name;
        private readonly IModel _model;
        private readonly IMessageTypeResolver _resolver;
        private readonly ISerializerFactory _serializerFactory;

        private readonly ISet<ConsumingPromise> _promises = new HashSet<ConsumingPromise>();

        // TODO: restore private
        internal MessageQueue(String name,
                              IModel model,
                              IMessageTypeResolver resolver,
                              ISerializerFactory serializerFactory)
        {
            _name = name;
            _model = model;
            _resolver = resolver;
            _serializerFactory = serializerFactory;
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

        public ConsumingPromise SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure)
        {
            return SubscribeByAtMostOnce(configure, NoFallbackStrategy.Instance);
        }

        public ConsumingPromise SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure,
                                                      IFallbackStrategy fallbackStrategy)
        {
            return Subscribe(configure,
                             (b, c) => new AtMostOnceConsumingPromise(this, b, c),
                             fallbackStrategy);
        }

        public ConsumingPromise SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure)
        {
            return SubscribeByAtLeastOnce(configure, NoFallbackStrategy.Instance);
        }

        public ConsumingPromise SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure,
                                                       IFallbackStrategy fallbackStrategy)
        {
            return Subscribe(configure,
                            (b, c) => new AtLeastOnceConsumingPromise(this, b, c),
                            fallbackStrategy);
        }

        internal static MessageQueue New(IModel model,
                                         IMessageTypeResolver resolver,
                                         ISerializerFactory serializerFactory,
                                         String name,
                                         Exchange exchange,
                                         String routingKey = "")
        {
            var queue = new MessageQueue(name, model, resolver, serializerFactory);

            exchange.Declare(model);
            model.QueueDeclare(name, true, false, false, new Dictionary<String, Object>());
            exchange.Bind(queue, model, routingKey);

            return queue;
        }

        private ConsumingPromise Subscribe(Action<SubscriptionConfiguration> configure,
                                           Func<IConsumedMessageBuilder, SubscriptionConfiguration, ConsumingPromise> func,
                                           IFallbackStrategy fallbackStrategy)
        {
            var builder = new ConsumedMessageBuilder(_serializerFactory, _resolver);
            var configuration = new SubscriptionConfiguration(fallbackStrategy);
            configure(configuration);
            var promise = func(builder, configuration);
            _promises.Add(promise);
            promise.Declare(_model); // TODO: should be deferred.
            return promise;
        }
    }
}