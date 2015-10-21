using System;
using System.Text;
using Carrot.Messaging;
using Carrot.Serialization;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class ConsumedMessageBuilder : IConsumedMessageBuilder
    {
        private readonly ISerializerFactory _serializerFactory;
        private readonly IMessageTypeResolver _resolver;

        internal ConsumedMessageBuilder(ISerializerFactory serializerFactory, IMessageTypeResolver resolver)
        {
            _serializerFactory = serializerFactory;
            _resolver = resolver;
        }

        public ConsumedMessageBase Build(BasicDeliverEventArgs args)
        {
            var messageType = _resolver.Resolve(args.BasicProperties.Type);

            if (messageType is EmptyMessageType)
                return new UnresolvedMessage(args);

            var serializer = _serializerFactory.Create(args.BasicProperties.ContentType);

            if (serializer is NullSerializer)
                return new UnsupportedMessage(args);

            Object content;

            try
            {
                content = serializer.Deserialize(args.Body,
                                                 messageType.RuntimeType,
                                                 Encoding(args));
            }
            catch { return new CorruptedMessage(args); }

            return new ConsumedMessage(content, args);
        }

        private static Encoding Encoding(BasicDeliverEventArgs args)
        {
            var encoding = args.BasicProperties.ContentEncoding;

            return encoding == null 
                       ? new UTF8Encoding(true) 
                       : System.Text.Encoding.GetEncoding(encoding);
        }
    }
}