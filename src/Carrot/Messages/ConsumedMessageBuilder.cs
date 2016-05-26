using System;
using Carrot.Configuration;
using Carrot.Serialization;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    internal class ConsumedMessageBuilder : IConsumedMessageBuilder
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
            var context = ConsumedMessageContext.FromBasicDeliverEventArgs(args);
            MessageBinding binding;

            try { binding = _resolver.Resolve(context); }
            catch (Exception) { return new UnresolvedMessage(args); }

            if (binding is EmptyMessageBinding)
                return new UnresolvedMessage(args);

            var serializer = context.CreateSerializer(_serializarionconfiguration);

            if (serializer is NullSerializer)
                return new UnsupportedMessage(args);

            try { return context.ToConsumedMessage(serializer, binding); }
            catch { return new CorruptedMessage(args); }
        }
    }
}