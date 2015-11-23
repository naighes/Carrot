using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    internal class OutboundMessageEnvelope<TMessage>
        where TMessage : class
    {
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
            var properties = _message.BuildBasicProperties(_configuration, _dateTimeProvider);
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

        private static IPublishResult Result(Task task)
        {
            if (task.Exception != null)
                return new FailurePublishing(task.Exception.GetBaseException());

            return SuccessfulPublishing.FromBasicProperties(task.AsyncState as IBasicProperties);
        }
    }
}