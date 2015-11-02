using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot
{
    public class AmqpConnection : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly ISerializerFactory _serializerFactory;
        private readonly INewId _newId;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMessageTypeResolver _resolver;

        public AmqpConnection(IConnection connection,
                              IModel model,
                              ISerializerFactory serializerFactory,
                              INewId newId,
                              IDateTimeProvider dateTimeProvider,
                              IMessageTypeResolver resolver)
        {
            _connection = connection;
            _model = model;
            _serializerFactory = serializerFactory;
            _newId = newId;
            _dateTimeProvider = dateTimeProvider;
            _resolver = resolver;
        }

        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "",
                                                           TaskFactory taskFactory = null) where TMessage : class
        {
            var envelope = new OutboundMessageEnvelope<TMessage>(message,
                                                                 _serializerFactory,
                                                                 _dateTimeProvider,
                                                                 _newId,
                                                                 _resolver);
            return envelope.PublishAsync(_model, exchange, routingKey, taskFactory);
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