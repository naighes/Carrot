using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot
{
    public interface IAmqpConnection : IDisposable
    {
        Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                    Exchange exchange,
                                                    String routingKey = "",
                                                    TaskFactory taskFactory = null) where TMessage : class;
    }

    public class AmqpConnection : IAmqpConnection
    {
        private readonly IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly IModel _outboundModel;
        private readonly ISerializerFactory _serializerFactory;
        private readonly INewId _newId;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IMessageTypeResolver _resolver;

        internal AmqpConnection(IConnection connection,
                                IEnumerable<ConsumerBase> consumers,
                                IModel outboundModel,
                                ISerializerFactory serializerFactory,
                                INewId newId,
                                IDateTimeProvider dateTimeProvider,
                                IMessageTypeResolver resolver)
        {
            _connection = connection;
            _consumers = consumers;
            _outboundModel = outboundModel;
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
            return envelope.PublishAsync(_outboundModel, exchange, routingKey, taskFactory);
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            if (_outboundModel != null)
                _outboundModel.Dispose();

            if (_connection != null)
                _connection.Dispose();
        }
    }
}