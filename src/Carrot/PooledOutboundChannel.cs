using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot
{
    public class PooledOutboundChannel : IOutboundChannel
    {
        private readonly IOutboundChannel _channel;
        private readonly Action<IOutboundChannel> _die;

        public PooledOutboundChannel(IOutboundChannel channel, Action<IOutboundChannel> die)
        {
            _channel = channel;
            _die = die;
        }

        public void Dispose()
        {
            _die(_channel);
        }

        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source, Exchange exchange, string routingKey) where TMessage : class
        {
            return _channel.PublishAsync(source, exchange, routingKey);
        }

        public Task<IPublishResult> ForwardAsync(ConsumedMessageBase message, Exchange exchange, string routingKey)
        {
            return _channel.ForwardAsync(message, exchange, routingKey);
        }
    }
}