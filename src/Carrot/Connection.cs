using System.Collections.Generic;
using System.Threading.Tasks;
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
                                                           string routingKey = "")
            where TMessage : class
        {
            using (var outboundChannel = _outboundChannelPool.Take())
            {
                return outboundChannel.PublishAsync(message, exchange, routingKey);
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