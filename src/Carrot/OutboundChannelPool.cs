using System.Collections.Concurrent;
using Carrot.Configuration;

namespace Carrot
{
    internal class OutboundChannelPool : IOutboundChannelPool
    {
        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly EnvironmentConfiguration _configuration;
        private readonly ConcurrentBag<IOutboundChannel> _channels;

        public OutboundChannelPool(RabbitMQ.Client.IConnection connection, EnvironmentConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
            _channels = new ConcurrentBag<IOutboundChannel>();
        }

        public IOutboundChannel Take()
        {
            IOutboundChannel channel;
            if (!_channels.TryTake(out channel))
            {
                var model = _connection.CreateModel();
                channel = _configuration.OutboundChannelBuilder(model, _configuration);
            }

            return new PooledOutboundChannel(channel, Add);
        }

        private void Add(IOutboundChannel channel)
        {
            _channels.Add(channel);
        }

        public void Dispose()
        {
            IOutboundChannel channel;
            while (_channels.TryTake(out channel))
            {
                channel.Dispose();
            }
        }
    }
}