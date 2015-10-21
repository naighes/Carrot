using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messaging
{
    public abstract class ConsumerBase : DefaultBasicConsumer
    {
        protected readonly IConsumedMessageBuilder Builder;
        protected readonly SubscriptionConfiguration Configuration;

        protected ConsumerBase(IModel model,
                               IConsumedMessageBuilder builder,
                               SubscriptionConfiguration configuration)
            : base(model)
        {
            Builder = builder;
            Configuration = configuration;
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

            ConsumeInternal(args);
        }

        protected abstract Task ConsumeInternal(BasicDeliverEventArgs args);
    }
}