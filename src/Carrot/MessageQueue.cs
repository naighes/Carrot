using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Fallback;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot
{
    public class MessageQueue
    {
        private readonly String _name;
        private readonly IModel _model;
        private readonly IMessageTypeResolver _resolver;

        private MessageQueue(String name, IModel model, IMessageTypeResolver resolver)
        {
            _name = name;
            _model = model;
            _resolver = resolver;
        }

        internal String Name
        {
            get { return _name; }
        }

        public void SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure)
        {
            SubscribeByAtMostOnce(configure, NoFallbackStrategy.Instance);
        }

        public void SubscribeByAtMostOnce(Action<SubscriptionConfiguration> configure,
                                          IFallbackStrategy fallbackStrategy)
        {
            Subscribe(configure, (b, c) => new AtMostOnceConsumer(_model, b, c), fallbackStrategy);
        }

        public void SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure)
        {
            SubscribeByAtLeastOnce(configure, NoFallbackStrategy.Instance);
        }

        public void SubscribeByAtLeastOnce(Action<SubscriptionConfiguration> configure,
                                           IFallbackStrategy fallbackStrategy)
        {
            Subscribe(configure, (b, c) => new AtLeastOnceConsumer(_model, b, c), fallbackStrategy);
        }

        internal static MessageQueue New(IModel model,
                                         IMessageTypeResolver resolver,
                                         String name,
                                         Exchange exchange,
                                         String routingKey = "")
        {
            var queue = new MessageQueue(name, model, resolver);

            exchange.Declare(model);
            model.QueueDeclare(name, true, false, false, new Dictionary<String, Object>());
            exchange.Bind(queue, model, routingKey);

            return queue;
        }

        private void Subscribe(Action<SubscriptionConfiguration> configure,
                               Func<IConsumedMessageBuilder, SubscriptionConfiguration, ConsumerBase> func,
                               IFallbackStrategy fallbackStrategy)
        {
            var configuration = new SubscriptionConfiguration(fallbackStrategy);
            configure(configuration);
            var builder = new ConsumedMessageBuilder(new SerializerFactory(), _resolver);

            _model.BasicConsume(_name, false, func(builder, configuration));
        }
    }
}