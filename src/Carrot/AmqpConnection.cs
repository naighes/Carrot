using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    using Carrot.Configuration;

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
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ChannelConfiguration _configuration;

        internal AmqpConnection(IConnection connection,
                                IEnumerable<ConsumerBase> consumers,
                                IModel outboundModel,
                                IDateTimeProvider dateTimeProvider,
                                ChannelConfiguration configuration)
        {
            _connection = connection;
            _consumers = consumers;
            _outboundModel = outboundModel;
            _dateTimeProvider = dateTimeProvider;
            _configuration = configuration;
        }

        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "",
                                                           TaskFactory taskFactory = null) where TMessage : class
        {
            var envelope = new OutboundMessageEnvelope<TMessage>(message, _dateTimeProvider, _configuration);
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