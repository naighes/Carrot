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

        private MessageQueue(String name, IModel model)
        {
            _name = name;
            _model = model;
        }

        public static MessageQueue New(IModel model,
                                       String exchange,
                                       String name,
                                       String routingKey)
        {
            model.QueueDeclare(name, true, false, false, new Dictionary<String, Object>());
            model.QueueBind(name, exchange, routingKey, new Dictionary<String, Object>());
            model.ExchangeDeclare(exchange, "direct", true);
            return new MessageQueue(name, model);
        }

        public void Config(Action<SubscriptionConfiguration> configure)
        {
            var configuration = new SubscriptionConfiguration();
            configure(configuration);
            var builder = new ConsumedMessageBuilder(new SerializerFactory(),
                                                     new MessageBindingResolver(typeof(Object).Assembly)); // TODO
            _model.BasicConsume(_name, 
                                false, 
                                new AtLeastOnceConsumer(_model, builder, configuration));
        }
    }
}