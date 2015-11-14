using System.Text;
using Carrot.Configuration;
using Carrot.Serialization;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class ConsumedMessageBuilder : IConsumedMessageBuilder
    {
        private readonly SerializationConfiguration _serializarionconfiguration;
        private readonly IMessageTypeResolver _resolver;

        internal ConsumedMessageBuilder(SerializationConfiguration serializarionconfiguration,
                                        IMessageTypeResolver resolver)
        {
            _serializarionconfiguration = serializarionconfiguration;
            _resolver = resolver;
        }

        public ConsumedMessageBase Build(BasicDeliverEventArgs args)
        {
            var binding = _resolver.Resolve(args.BasicProperties.Type);

            if (binding is EmptyMessageBinding)
                return new UnresolvedMessage(args);

            var serializer = _serializarionconfiguration.Create(args.BasicProperties.ContentType);

            if (serializer is NullSerializer)
                return new UnsupportedMessage(args);

            try { return Content(args, serializer, binding); }
            catch { return new CorruptedMessage(args); }
        }

        private static ConsumedMessage Content(BasicDeliverEventArgs args,
                                               ISerializer serializer,
                                               MessageBinding messageBinding)
        {
            return new ConsumedMessage(serializer.Deserialize(args.Body,
                                                              messageBinding.RuntimeType,
                                                              Encoding(args)),
                                       args);
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