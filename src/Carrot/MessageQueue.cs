using System;
using System.Collections.Generic;
using Carrot.Configuration;
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

        public void ConfigureSubscriptions(Action<SubscriptionConfiguration> configure)
        {
            var configuration = new SubscriptionConfiguration();
            configure(configuration);
            var builder = new ConsumedMessageBuilder(new SerializerFactory(), _resolver);
            _model.BasicConsume(_name,
                                false,
                                new AtLeastOnceConsumer(_model, builder, configuration));
        }
    }
}