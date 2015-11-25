using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class AmqpConnection : IAmqpConnection
    {
        protected readonly ChannelConfiguration Configuration;

        private readonly IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly ReliableOutboundChannel _outboundChannel;
        private readonly IDateTimeProvider _dateTimeProvider;

        internal AmqpConnection(IConnection connection,
                                IEnumerable<ConsumerBase> consumers,
                                ReliableOutboundChannel outboundChannel,
                                IDateTimeProvider dateTimeProvider,
                                ChannelConfiguration configuration)
        {
            _connection = connection;
            _consumers = consumers;
            _outboundChannel = outboundChannel;
            _dateTimeProvider = dateTimeProvider;
            Configuration = configuration;
        }
        
        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "")
            where TMessage : class
        {
            var properties = message.BuildBasicProperties(Configuration.MessageTypeResolver,
                                                          _dateTimeProvider,
                                                          Configuration.IdGenerator);
            var body = properties.CreateEncoding()
                                 .GetBytes(properties.CreateSerializer(Configuration.SerializationConfiguration)
                                 .Serialize(message.Content));
            var envelope = _outboundChannel.BuildEnvelope(properties, body, exchange, routingKey, message);
            return _outboundChannel.PublishAsync(envelope);
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            _outboundChannel?.Dispose();
            _connection?.Dispose();
        }
    }
}