using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AmqpConnection : IAmqpConnection
    {
        protected readonly ChannelConfiguration Configuration;

        private readonly IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly IModel _outboundModel;
        private readonly IDateTimeProvider _dateTimeProvider;

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
            Configuration = configuration;

            _outboundModel.BasicAcks += OnOutboundModelBasicAcks;
            _outboundModel.BasicNacks += OnOutboundModelBasicNacks;
            _outboundModel.BasicReturn += OnOutboundModelBasicReturn;
        }

        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "",
                                                           TaskFactory taskFactory = null) where TMessage : class
        {
            var tag = _outboundModel.NextPublishSeqNo;
            var envelope = new OutboundMessageEnvelope<TMessage>(message, _dateTimeProvider, tag, Configuration);
            return envelope.PublishAsync(_outboundModel, exchange, routingKey, taskFactory);
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            if (_outboundModel != null)
            {
                _outboundModel.WaitForConfirms(TimeSpan.FromSeconds(30d)); // TODO: timeout should not be hardcodeds

                _outboundModel.BasicAcks -= OnOutboundModelBasicAcks;
                _outboundModel.BasicNacks -= OnOutboundModelBasicNacks;
                _outboundModel.BasicReturn -= OnOutboundModelBasicReturn;

                _outboundModel.Dispose();
            }

            _connection?.Dispose();
        }

        protected virtual void OnOutboundModelBasicReturn(Object sender, BasicReturnEventArgs args) { }

        protected virtual void OnOutboundModelBasicNacks(Object sender, BasicNackEventArgs args) { }

        protected virtual void OnOutboundModelBasicAcks(Object sender, BasicAckEventArgs args) { }
    }
}