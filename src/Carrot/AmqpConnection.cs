using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class AmqpConnection : IAmqpConnection
    {
        protected readonly ChannelConfiguration Configuration;

        private readonly IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly OutboundChannel _outboundChannel;
        private readonly IDateTimeProvider _dateTimeProvider;

        internal AmqpConnection(IConnection connection,
                                IEnumerable<ConsumerBase> consumers,
                                OutboundChannel outboundChannel,
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
                                                           String routingKey = "",
                                                           TaskFactory taskFactory = null) where TMessage : class
        {
            var tag = _outboundChannel.Model.NextPublishSeqNo;
            var properties = message.BuildBasicProperties(Configuration.MessageTypeResolver,
                                                          _dateTimeProvider,
                                                          Configuration.IdGenerator);
            var envelope = new OutboundMessageEnvelope<TMessage>(properties,
                                                                 message.Content,
                                                                 tag,
                                                                 Configuration.SerializationConfiguration);
            return envelope.PublishAsync(_outboundChannel, exchange, routingKey, taskFactory);
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