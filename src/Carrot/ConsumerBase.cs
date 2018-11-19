using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public abstract class ConsumerBase : DefaultBasicConsumer, IDisposable
    {
        protected readonly ConsumingConfiguration Configuration;
        protected readonly IInboundChannel InboundChannel;
        protected readonly IOutboundChannelPool OutboundChannelPool;

        private readonly Queue _queue;
        private readonly IConsumedMessageBuilder _builder;

        protected internal ConsumerBase(IInboundChannel inboundChannel,
                                        IOutboundChannelPool outboundChannelPool,
                                        Queue queue,
                                        IConsumedMessageBuilder builder,
                                        ConsumingConfiguration configuration)
        {
            InboundChannel = inboundChannel;
            OutboundChannelPool = outboundChannelPool;
            _queue = queue;
            _builder = builder;
            Configuration = configuration;

            ConsumerCancelled += OnConsumerCancelled;
        }

        public override void HandleBasicDeliver(String consumerTag,
                                                UInt64 deliveryTag,
                                                Boolean redelivered,
                                                String exchange,
                                                String routingKey,
                                                IBasicProperties properties,
                                                Byte[] body)
        {
            base.HandleBasicDeliver(consumerTag,
                                    deliveryTag,
                                    redelivered,
                                    exchange,
                                    routingKey,
                                    properties,
                                    body);

            var args = new BasicDeliverEventArgs
                           {
                               ConsumerTag = consumerTag,
                               DeliveryTag = deliveryTag,
                               Redelivered = redelivered,
                               Exchange = exchange,
                               RoutingKey = routingKey,
                               BasicProperties = properties,
                               Body = body
                           };

            ConsumeInternalAsync(args);
        }

        public void Dispose()
        {
            if (InboundChannel == null)
                return;

            ConsumerCancelled -= OnConsumerCancelled;

            InboundChannel.Dispose();
        }

        internal void Declare(IModel model)
        {
            model.BasicConsume(_queue.Name, false, this);
        }

        protected internal virtual Task<AggregateConsumingResult> ConsumeAsync(BasicDeliverEventArgs args)
        {
            var message = _builder.Build(args);
            return message.ConsumeAsync(Configuration.FindSubscriptions(message));
        }

        protected abstract Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args);

        protected virtual void OnConsumerCancelled(Object sender, ConsumerEventArgs args) { }
    }
}