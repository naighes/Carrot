using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Carrot.Extensions;
using Carrot.Messaging;
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

        internal OutboundMessageEnvelope(OutboundMessage<TMessage> message,
                                         ISerializerFactory serializerFactory,
                                         IDateTimeProvider dateTimeProvider,
                                         INewId newId)
        {
            _message = message;
            _serializerFactory = serializerFactory;
            _dateTimeProvider = dateTimeProvider;
            _newId = newId;
        }

        internal Task<IPublishResult> PublishAsync(IModel model, String exchange, String routingKey = "")
        {
            var properties = BuildBasicProperties();
            HydrateProperties(properties);

            var encoding = Encoding.GetEncoding(properties.ContentEncoding);
            var serializer = _serializerFactory.Create(properties.ContentType);

            return Task.Factory
                       .StartNew(_ =>
                                 {
                                     model.BasicPublish(exchange,
                                                        routingKey,
                                                        (IBasicProperties)_,
                                                        encoding.GetBytes(serializer.Serialize(_message.Content)));
                                 },
                                 properties)
                       .ContinueWith<IPublishResult>(Result);
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

        protected virtual void HydrateProperties(IBasicProperties properties)
        {
            _message.HydrateProperties(properties);
            properties.MessageId = _newId.Next();
            properties.Timestamp = new AmqpTimestamp(_dateTimeProvider.UtcNow().ToUnixTimestamp());
            properties.Type = MessageType();

            if (properties.ContentEncoding == null)
                properties.ContentEncoding = DefaultContentEncoding;

            if (properties.ContentType == null)
                properties.ContentType = DefaultContentType;
        }

        // TODO: should depend on resolver.
        private static String MessageType()
        {
            var attribute = typeof(TMessage).GetCustomAttribute<MessageBindingAttribute>();

            return attribute != null 
                ? attribute.MessageType 
                : String.Format("urn:message:{0}", typeof(TMessage).FullName);
        }
    }
}