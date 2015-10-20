using System;
using System.Collections.Generic;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot.Messaging
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

        internal static MessageQueue New(IModel model,
                                         IMessageTypeResolver resolver,
                                         String name,
                                         String exchange,
                                         String routingKey)
        {
            model.ExchangeDeclare(exchange, "direct", true);
            model.QueueDeclare(name, true, false, false, new Dictionary<String, Object>());
            model.QueueBind(name, exchange, routingKey, new Dictionary<String, Object>());

            return new MessageQueue(name, model, resolver);
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