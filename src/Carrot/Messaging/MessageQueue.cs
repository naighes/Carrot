namespace Carrot.Messaging
{
    using System;
    using System.Collections.Generic;

    using Carrot.Serialization;

    using RabbitMQ.Client;

    public class MessageQueue
    {
        private readonly String _name;
        private readonly IModel _model;

        private MessageQueue(String name, IModel model)
        {
            this._name = name;
            this._model = model;
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
            this._model.BasicConsume(this._name, 
                                false, 
                                new AsyncBasicConsumer(this._model,
                                                       new AppDomainAssembliesResolver(typeof(Object).Assembly), // TODO
                                                       new SerializerFactory(),
                                                       configuration));
        }
    }
}