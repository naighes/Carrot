using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Carrot.Messages
{
    internal class OutboundMessageEnvelope<TMessage>
        where TMessage : class
    {
        private const String DefaultContentEncoding = "UTF-8";
        private const String DefaultContentType = "application/json";

        private readonly OutboundMessage<TMessage> _message;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly UInt64 _tag;
        private readonly ChannelConfiguration _configuration;

        internal OutboundMessageEnvelope(OutboundMessage<TMessage> message,
                                         IDateTimeProvider dateTimeProvider,
                                         UInt64 tag,
                                         ChannelConfiguration configuration)
        {
            _message = message;
            _dateTimeProvider = dateTimeProvider;
            _tag = tag;
            _configuration = configuration;
        }

        internal Task<IPublishResult> PublishAsync(OutboundChannel channel,
                                                   Exchange exchange,
                                                   String routingKey = "",
                                                   TaskFactory taskFactory = null)
        {
            var properties = BuildBasicProperties();
            HydrateProperties(properties);

            var factory = taskFactory ?? Task.Factory;

            return factory.StartNew(_ =>
                                    {
                                        channel.Model.BasicPublish(exchange.Name,
                                                                   routingKey,
                                                                   false,
                                                                   false,
                                                                   (IBasicProperties)_,
                                                                   properties.CreateEncoding()
                                                                             .GetBytes(properties.CreateSerializer(_configuration.SerializationConfiguration)
                                                                                                 .Serialize(_message.Content)));
                                    },
                                    properties)
                          .ContinueWith(Result);
        }

        protected virtual void HydrateProperties(IBasicProperties properties)
        {
            _message.HydrateProperties(properties);

            properties.MessageId = _configuration.IdGenerator.Next();
            properties.Timestamp = new AmqpTimestamp(_dateTimeProvider.UtcNow().ToUnixTimestamp());
            var binding = _configuration.MessageTypeResolver.Resolve<TMessage>();
            properties.Type = binding.RawName;

            if (properties.ContentEncoding == null)
                properties.ContentEncoding = DefaultContentEncoding;

            if (properties.ContentType == null)
                properties.ContentType = DefaultContentType;

            if (binding.ExpiresAfter.HasValue)
                properties.Expiration = binding.ExpiresAfter
                                               .GetValueOrDefault()
                                               .TotalMilliseconds
                                               .ToString(CultureInfo.InvariantCulture);
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