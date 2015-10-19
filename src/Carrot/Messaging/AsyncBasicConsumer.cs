using System;
using System.Text;
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
            var e = new BasicDeliverEventArgs
                        {
                            ConsumerTag = consumerTag,
                            DeliveryTag = deliveryTag,
                            Redelivered = redelivered,
                            Exchange = exchange,
                            RoutingKey = routingKey,
                            BasicProperties = properties,
                            Body = body
                        };

            var message = ReadMessage(e);

            message.ConsumeAsync(_configuration);
        }

        private ConsumedMessageBase ReadMessage(BasicDeliverEventArgs args)
        {
            var headers = HeaderCollection.Parse(args);
            var messageType = _resolver.Resolve(args.BasicProperties.Type);

            if (messageType is EmptyMessageType)
                return new UnresolvedMessage(headers, args);

            var serializer = _serializerFactory.Create(args.BasicProperties.ContentType);

            if (serializer is NullSerializer)
                return new UnsupportedMessage(headers, args);

            Object content;

            try
            {
                content = serializer.Deserialize(args.Body,
                                                 messageType.RuntimeType,
                                                 Encoding.GetEncoding(args.BasicProperties.ContentEncoding));
            }
            catch { return new CorruptedMessage(headers, args); }

            return new ConsumedMessage(content, headers, args);
        }
    }
}