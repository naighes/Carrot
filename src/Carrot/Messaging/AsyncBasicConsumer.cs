using System;
using Carrot.Messages;
using Carrot.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messaging
{
    public class AsyncBasicConsumer : DefaultBasicConsumer
    {
        private readonly IMessageTypeResolver _resolver;
        private readonly ISerializerFactory _serializerFactory;
        private readonly SubscriptionConfiguration _configuration;

        // TODO: make it internal and replace resolver and serializerFactory with builder.
        public AsyncBasicConsumer(IModel model, 
                                  IMessageTypeResolver resolver,
                                  ISerializerFactory serializerFactory,
                                  SubscriptionConfiguration configuration)
            : base(model)
        {
            _resolver = resolver;
            _serializerFactory = serializerFactory;
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

            ReadMessage(args).ConsumeAsync(_configuration);
        }

        private ConsumedMessageBase ReadMessage(BasicDeliverEventArgs args)
        {
            return new ConsumedMessageBuilder(_serializerFactory, _resolver).Build(args);
        }
    }
}