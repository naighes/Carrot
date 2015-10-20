using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RabbitMQ.Client.Framing.Impl;

namespace Carrot.Messaging
{
    public class AmqpChannel : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;

        private AmqpChannel(IConnection connection, IModel model)
        {
            _connection = connection;
            _model = model;
        }

        public static AmqpChannel New(String endpointUrl)
        {
            var connectionFactory = new ConnectionFactory
                                        {
                                            Uri = endpointUrl,
                                            AutomaticRecoveryEnabled = true,
                                            TopologyRecoveryEnabled = true
                                        };
            var connection = (AutorecoveringConnection)connectionFactory.CreateConnection();
            return new AmqpChannel(connection, CreateModel(connection));
        }

        public MessageQueue Bind(IMessageTypeResolver resolver, 
                                 String name, 
                                 String exchange, 
                                 String routingKey = "")
        {
            return MessageQueue.New(_model, resolver, name, exchange, routingKey);
        }

        public void Publish<TMessage>(TMessage message, String exchange, String routingKey = "")
        {
            // TODO: ensure exchange
            var properties = new BasicProperties
            {
                ContentEncoding = "UTF-8",
                ContentType = "application/json",
                Type = typeof(TMessage).GetCustomAttribute<MessageBindingAttribute>().MessageType // TODO: cache
            };
            _model.BasicPublish(exchange,
                                routingKey,
                                properties,
                                Encoding.GetEncoding(properties.ContentEncoding).GetBytes(JsonConvert.SerializeObject(message)));
        }

        private static IModel CreateModel(IConnection connection)
        {
            var model = connection.CreateModel();
            model.ConfirmSelect();
            return model;
        }

        public void Dispose()
        {
            if (_model != null)
                _model.Dispose();

            if (_connection != null)
                _connection.Dispose();
        }
    }
}