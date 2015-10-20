using System;
using System.Reflection;
using System.Text;
using Carrot.Extensions;
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
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INewId _newId;

        private AmqpChannel(IConnection connection, 
                            IModel model, 
                            IDateTimeProvider dateTimeProvider,
                            INewId newId)
        {
            _connection = connection;
            _model = model;
            _dateTimeProvider = dateTimeProvider;
            _newId = newId;
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
            return new AmqpChannel(connection, 
                                   CreateModel(connection), 
                                   new DateTimeProvider(),
                                   new NewGuid());
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
                MessageId = _newId.Next(),
                Type = typeof(TMessage).GetCustomAttribute<MessageBindingAttribute>().MessageType, // TODO: cache
                Timestamp = new AmqpTimestamp(_dateTimeProvider.UtcNow().ToUnixTimestamp())
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