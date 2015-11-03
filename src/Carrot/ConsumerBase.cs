using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public abstract class ConsumerBase : DefaultBasicConsumer
    {
        private readonly IConsumedMessageBuilder _builder;
        private readonly SubscriptionConfiguration _configuration;

        protected internal ConsumerBase(IModel model,
                                        IConsumedMessageBuilder builder,
                                        SubscriptionConfiguration configuration)
            : base(model)
        {
            _builder = builder;
            _configuration = configuration;
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

        protected internal Task<AggregateConsumingResult> ConsumeAsync(BasicDeliverEventArgs args)
        {
            return _builder.Build(args).ConsumeAsync(_configuration);
        }

        protected abstract Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args);
    }
}