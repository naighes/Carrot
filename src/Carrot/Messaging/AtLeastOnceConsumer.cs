using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messaging
{
    public class AtLeastOnceConsumer : DefaultBasicConsumer
    {
        private readonly IModel _model;
        private readonly IConsumedMessageBuilder _builder;

        private readonly SubscriptionConfiguration _configuration;

        internal AtLeastOnceConsumer(IModel model, 
                                     IConsumedMessageBuilder builder,
                                     SubscriptionConfiguration configuration)
            : base(model)
        {
            _model = model;
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

            ConsumeInternal(args);
        }

        protected virtual Task ConsumeInternal(BasicDeliverEventArgs args)
        {
            return _builder.Build(args)
                           .ConsumeAsync(_configuration)
                           .ContinueWith(_ => _.Result.Reply(_model));
        }
    }
}