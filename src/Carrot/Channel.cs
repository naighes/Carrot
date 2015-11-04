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

        private readonly ISet<Queue> _queues = new HashSet<Queue>();

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

        public Queue DeclareQueue(String name)
        {
            return DeclareQueue(name, false);
        }

        public Queue DeclareDurableQueue(String name)
        {
            return DeclareQueue(name, true);
        }

        public IAmqpConnection Connect()
        {
            var connection = CreateConnection();
            var model = CreateModel(connection, _prefetchSize, _prefetchCount);

            foreach (var queue in _queues)
                queue.Declare(model, new ConsumedMessageBuilder(_serializerFactory, _resolver));

            //foreach (var binding in _exchanges)
            //    binding.Value.Declare(model, new ConsumedMessageBuilder(_serializerFactory, _resolver));

            return new AmqpConnection(connection,
                                      model,
                                      _serializerFactory,
                                      _newId,
                                      _dateTimeProvider,
                                      _resolver);
        }

        protected internal virtual IConnection CreateConnection()
        {
            return new ConnectionFactory
                       {
                           Uri = _endpointUrl,
                           AutomaticRecoveryEnabled = true,
                           TopologyRecoveryEnabled = true
                       }.CreateConnection();
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

        private Queue DeclareQueue(String name, Boolean isDurable)
        {
            var queue = new Queue(name, isDurable);
            _queues.Add(queue);
            return queue;
        }
    }
}