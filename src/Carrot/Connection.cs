using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Messages;

namespace Carrot
{
    public class Connection : IConnection
    {
        protected readonly EnvironmentConfiguration Configuration;

        private readonly RabbitMQ.Client.IConnection _connection;
        private readonly IEnumerable<ConsumerBase> _consumers;
        private readonly IOutboundChannel _channel;
        private readonly IDateTimeProvider _dateTimeProvider;

        internal Connection(RabbitMQ.Client.IConnection connection,
                            IEnumerable<ConsumerBase> consumers,
                            IOutboundChannel channel,
                            IDateTimeProvider dateTimeProvider,
                            EnvironmentConfiguration configuration)
        {
            _connection = connection;
            _consumers = consumers;
            _channel = channel;
            _dateTimeProvider = dateTimeProvider;
            Configuration = configuration;
        }
        
        public Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> message,
                                                           Exchange exchange,
                                                           String routingKey = "")
            where TMessage : class
        {
            var properties = message.BuildBasicProperties(Configuration.MessageTypeResolver,
                                                          _dateTimeProvider,
                                                          Configuration.IdGenerator);
            var body = properties.CreateEncoding()
                                 .GetBytes(properties.CreateSerializer(Configuration.SerializationConfiguration)
                                                     .Serialize(message.Content));
            return _channel.PublishAsync(message, properties, body, exchange, routingKey);
        }

        public void Dispose()
        {
            foreach (var consumer in _consumers)
                consumer.Dispose();

            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}