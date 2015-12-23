using System;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Serialization;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class ConsumedMessageContext
    {
        public String Source => _args.Exchange;

        public String ContentType => _args.BasicProperties.ContentTypeOrDefault(SerializationConfiguration.DefaultContentType);

        public String ContentEncoding => _args.BasicProperties.ContentEncodingOrDefault(SerializationConfiguration.DefaultContentEncoding);

        public String MessageType => _args.BasicProperties.Type;

        private readonly BasicDeliverEventArgs _args;

        private ConsumedMessageContext(BasicDeliverEventArgs args)
        {
            _args = args;
        }

        public static ConsumedMessageContext FromBasicDeliverEventArgs(BasicDeliverEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            return new ConsumedMessageContext(args);
        }

        internal ConsumedMessage ToConsumedMessage(ISerializer serializer, MessageBinding messageBinding)
        {
            return new ConsumedMessage(serializer.Deserialize(_args.Body,
                                                              messageBinding.RuntimeType,
                                                              _args.BasicProperties.CreateEncoding()),
                                       _args);
        }

        internal ISerializer CreateSerializer(SerializationConfiguration configuration)
        {
            return _args.BasicProperties.CreateSerializer(configuration);
        }
    }
}