using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;

namespace Carrot
{
    public interface IChannel : IDisposable
    {
        Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                    Exchange exchange,
                                                    String routingKey = "",
                                                    TaskFactory taskFactory = null) where TMessage : class;

        MessageQueue Bind(String name, Exchange exchange, String routingKey = "");
    }

    public class AmqpChannel : IChannel
    {
        private readonly IConnection _connection;
        private readonly IModel _model;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INewId _newId;
        private readonly ISerializerFactory _serializerFactory;
        private readonly IMessageTypeResolver _resolver;

        private AmqpChannel(IConnection connection,
                            IModel model,
                            IDateTimeProvider dateTimeProvider,
                            INewId newId,
                            ISerializerFactory serializerFactory,
                            IMessageTypeResolver resolver)
        {
            _connection = connection;
            _model = model;
            _dateTimeProvider = dateTimeProvider;
            _newId = newId;
            _serializerFactory = serializerFactory;
            _resolver = resolver;
        }

        public static AmqpChannel New(String endpointUrl,
                                      IMessageTypeResolver resolver,
                                      UInt32 prefetchSize = 0,
                                      UInt16 prefetchCount = 0)
        {
            var connectionFactory = new ConnectionFactory
                                        {
                                            Uri = endpointUrl,
                                            AutomaticRecoveryEnabled = true,
                                            TopologyRecoveryEnabled = true
                                        };
            var connection = (AutorecoveringConnection)connectionFactory.CreateConnection();

            return new AmqpChannel(connection,
                                   CreateModel(connection, prefetchSize, prefetchCount),
                                   new DateTimeProvider(),
                                   new NewGuid(),
                                   new SerializerFactory(),
                                   resolver);
        }

        public MessageQueue Bind(String name,
                                 Exchange exchange,
                                 String routingKey = "")
        {
            return MessageQueue.New(_model, _resolver, _serializerFactory, name, exchange, routingKey);
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

        private static IModel CreateModel(IConnection connection, UInt32 prefetchSize, UInt16 prefetchCount)
        {
            var model = connection.CreateModel();
            model.ConfirmSelect(); // TODO: should not be by default.
            model.BasicQos(prefetchSize, prefetchCount, false);
            return model;
        }
    }
}