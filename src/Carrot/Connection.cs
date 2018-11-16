using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;

namespace Carrot
{
    public class Connection : IConnection
    {
        private volatile int _count; // TODO: remove me, for testing only

        protected readonly EnvironmentConfiguration Configuration;

        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly IOutboundChannelPool _outboundChannelPool;
        private readonly IOutboundChannel _outboundChannel;

        internal Connection(RabbitMQ.Client.IConnection connection,
                            IEnumerable<ConsumerBase> consumers,
                            IOutboundChannelPool outboundChannelPool,
                            IOutboundChannel outboundChannel,
                            EnvironmentConfiguration configuration)
        {
            _connection = connection;
            _consumers = consumers;
            _outboundChannelPool = outboundChannelPool;
            _outboundChannel = outboundChannel;
            Configuration = configuration;
        }

        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
            Exchange exchange,
            String routingKey = "")
            where TMessage : class
        {
            _count++;
            //return Task.Run(() =>
            //{
                var outboundChannel = _outboundChannelPool.Take();
                try
                {
                    //Console.WriteLine($"Before _outboundChannel.PublishAsync to {_count}");
                    var publishAsync = outboundChannel.PublishAsync(message, exchange, routingKey);
                    //Console.WriteLine($"After _outboundChannel.PublishAsync to {_count}");
                    return publishAsync;
                }
                finally
                {
                    _outboundChannelPool.Add(outboundChannel);
                }
            //});
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            _outboundChannelPool?.Dispose();
            _outboundChannel?.Dispose();
            _connection?.Dispose();
        }
    }
}