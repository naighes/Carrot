using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messaging
{
    public class AtLeastOnceConsumer : ConsumerBase
    {
        internal AtLeastOnceConsumer(IModel model, 
                                     IConsumedMessageBuilder builder,
                                     SubscriptionConfiguration configuration)
            : base(model, builder, configuration)
        {
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

        protected override Task ConsumeInternal(BasicDeliverEventArgs args)
        {
            return Builder.Build(args)
                          .ConsumeAsync(Configuration)
                          .ContinueWith(_ => _.Result.Reply(Model));
        }
    }
}