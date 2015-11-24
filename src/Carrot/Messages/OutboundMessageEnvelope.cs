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
        private readonly IBasicProperties _properties;
        private readonly TMessage _content;
        private readonly UInt64 _tag;
        private readonly SerializationConfiguration _serializationConfiguration;

        internal OutboundMessageEnvelope(IBasicProperties properties,
                                         TMessage content,
                                         UInt64 tag,
                                         SerializationConfiguration serializationConfiguration)
        {
            _properties = properties;
            _content = content;
            _tag = tag;
            _serializationConfiguration = serializationConfiguration;
        }

        internal Task<IPublishResult> PublishAsync(OutboundChannel channel,
                                                   Exchange exchange,
                                                   String routingKey = "",
                                                   TaskFactory taskFactory = null)
        {
            var factory = taskFactory ?? Task.Factory;
            var body = _properties.CreateEncoding()
                                  .GetBytes(_properties.CreateSerializer(_serializationConfiguration)
                                  .Serialize(_content));

            return factory.StartNew(_ =>
                                    {
                                        channel.Model.BasicPublish(exchange.Name,
                                                                   routingKey,
                                                                   false,
                                                                   false,
                                                                   (IBasicProperties)_,
                                                                   body);
                                    },
                                    _properties)
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