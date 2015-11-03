using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;

namespace Carrot
{
    public interface IChannel
    {
        MessageQueue Bind(String name, Exchange exchange, String routingKey = "");

        IAmqpConnection Connect();
    }

    public class Channel : IChannel
    {
        private readonly String _endpointUrl;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INewId _newId;
        private readonly ISerializerFactory _serializerFactory;
        private readonly IMessageTypeResolver _resolver;
        private readonly UInt32 _prefetchSize;
        private readonly UInt16 _prefetchCount;

        private readonly IDictionary<String, Exchange> _exchanges = new Dictionary<String, Exchange>();

        protected internal Channel(String endpointUrl,
                                   IDateTimeProvider dateTimeProvider,
                                   INewId newId,
                                   ISerializerFactory serializerFactory,
                                   IMessageTypeResolver resolver,
                                   UInt32 prefetchSize,
                                   UInt16 prefetchCount)
        {
            _endpointUrl = endpointUrl;
            _dateTimeProvider = dateTimeProvider;
            _newId = newId;
            _serializerFactory = serializerFactory;
            _resolver = resolver;
            _prefetchSize = prefetchSize;
            _prefetchCount = prefetchCount;
        }

        public static Channel New(String endpointUrl,
                                  IMessageTypeResolver resolver,
                                  UInt32 prefetchSize = 0,
                                  UInt16 prefetchCount = 0)
        {
            return new Channel(endpointUrl,
                               new DateTimeProvider(),
                               new NewGuid(),
                               new SerializerFactory(),
                               resolver,
                               prefetchSize,
                               prefetchCount);
        }

        public MessageQueue Bind(String name,
                                 Exchange exchange,
                                 String routingKey = "")
        {
            var builder = new ConsumedMessageBuilder(_serializerFactory, _resolver);
            var queue = MessageQueue.New(name, builder);

            if (!_exchanges.ContainsKey(exchange.Name))
                _exchanges.Add(exchange.Name, exchange);

            _exchanges[exchange.Name].Bind(queue, routingKey);

            return queue;
        }

        public IAmqpConnection Connect()
        {
            var connection = CreateConnection();
            var model = CreateModel(connection, _prefetchSize, _prefetchCount);

            foreach (var binding in _exchanges)
                binding.Value.Declare(model);

            return new AmqpConnection(connection,
                                      model,
                                      _serializerFactory,
                                      _newId,
                                      _dateTimeProvider,
                                      _resolver);
        }

        protected internal virtual IConnection CreateConnection()
        {
            var connectionFactory = new ConnectionFactory
                                        {
                                            Uri = _endpointUrl,
                                            AutomaticRecoveryEnabled = true,
                                            TopologyRecoveryEnabled = true
                                        };
            return connectionFactory.CreateConnection();
        }

        protected internal virtual IModel CreateModel(IConnection connection,
                                                      UInt32 prefetchSize,
                                                      UInt16 prefetchCount)
        {
            var model = connection.CreateModel();
            model.ConfirmSelect();
            model.BasicQos(prefetchSize, prefetchCount, false);
            return model;
        }
    }
}