using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Carrot.Extensions;
using Carrot.Messaging;
using Carrot.Serialization;
using RabbitMQ.Client;

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
            var properties = _message.Headers.ToOutboundBasicProperties();

            EnrichProperties(properties);

            var encoding = Encoding.GetEncoding(properties.ContentEncoding);
            var serializer = _serializerFactory.Create(properties.ContentType);

            return Task.Factory
                       .StartNew(() =>
                                 {
                                     model.BasicPublish(exchange,
                                                        routingKey,
                                                        properties,
                                                        encoding.GetBytes(serializer.Serialize(_message.Content)));
                                 })
                       .ContinueWith<IPublishResult>(_ =>
                                                     {
                                                         if (_.Exception != null)
                                                             return new FailurePublishing(_.Exception.GetBaseException());

                                                         return SuccessfulPublishing.FromBasicProperties(properties);
                                                     });
        }

        protected virtual void EnrichProperties(IBasicProperties properties)
        {
            properties.MessageId = _newId.Next();
            properties.Timestamp = new AmqpTimestamp(_dateTimeProvider.UtcNow().ToUnixTimestamp());
            properties.Type = typeof(TMessage).GetCustomAttribute<MessageBindingAttribute>().MessageType;

            if (properties.ContentEncoding == null)
                properties.ContentEncoding = DefaultContentEncoding;

            if (properties.ContentType == null)
                properties.ContentType = DefaultContentType;
        }
    }
}