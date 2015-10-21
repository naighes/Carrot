using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Messages
{
    internal class OutboundMessageEnvelope<TMessage> where TMessage : class
    {
        private const String DefaultContentEncoding = "UTF-8";
        private const String DefaultContentType = "application/json";

        private readonly OutboundMessage<TMessage> _message;
        private readonly ISerializerFactory _serializerFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INewId _newId;
        private readonly IMessageTypeResolver _resolver;

        internal OutboundMessageEnvelope(OutboundMessage<TMessage> message,
                                         ISerializerFactory serializerFactory,
                                         IDateTimeProvider dateTimeProvider,
                                         INewId newId,
                                         IMessageTypeResolver resolver)
        {
            _message = message;
            _serializerFactory = serializerFactory;
            _dateTimeProvider = dateTimeProvider;
            _newId = newId;
            _resolver = resolver;
        }

        internal Task<IPublishResult> PublishAsync(IModel model, Exchange exchange, String routingKey = "")
        {
            var properties = BuildBasicProperties();
            HydrateProperties(properties);

            var encoding = Encoding.GetEncoding(properties.ContentEncoding);
            var serializer = _serializerFactory.Create(properties.ContentType);

            return Task.Factory
                       .StartNew(_ =>
                                 {
                                     model.BasicPublish(exchange.Name,
                                                        routingKey,
                                                        (IBasicProperties)_,
                                                        encoding.GetBytes(serializer.Serialize(_message.Content)));
                                 },
                                 properties)
                       .ContinueWith<IPublishResult>(Result);
        }

        protected virtual void HydrateProperties(IBasicProperties properties)
        {
            _message.HydrateProperties(properties);
            properties.MessageId = _newId.Next();
            properties.Timestamp = new AmqpTimestamp(_dateTimeProvider.UtcNow().ToUnixTimestamp());
            properties.Type = _resolver.Resolve<TMessage>().RawName;

            if (properties.ContentEncoding == null)
                properties.ContentEncoding = DefaultContentEncoding;

            if (properties.ContentType == null)
                properties.ContentType = DefaultContentType;
        }

        private static BasicProperties BuildBasicProperties()
        {
            return new BasicProperties { Headers = new Dictionary<String, Object>() };
        }

        private static IPublishResult Result(Task task)
        {
            if (task.Exception != null)
                return new FailurePublishing(task.Exception.GetBaseException());

            return SuccessfulPublishing.FromBasicProperties(task.AsyncState as IBasicProperties);
        }
    }
}