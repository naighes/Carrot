using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot
{
    public class Connection : IConnection
    {
        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly IOutboundChannelPool _outboundChannelPool;

        internal Connection(RabbitMQ.Client.IConnection connection,
                            IEnumerable<ConsumerBase> consumers,
                            IOutboundChannelPool outboundChannelPool)
        {
            _connection = connection;
            _consumers = consumers;
            _outboundChannelPool = outboundChannelPool;
        }

        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "")
            where TMessage : class
        {
            var outboundChannel = _outboundChannelPool.Take();
            try
            {
                return outboundChannel.PublishAsync(message, exchange, routingKey);
            }
            finally
            {
                _outboundChannelPool.Add(outboundChannel);
            }
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            _outboundChannelPool?.Dispose();
            _connection?.Dispose();
        }
    }
}