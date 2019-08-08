using System;
using Carrot.Messages;

namespace Carrot
{
    public class ConsumingContext
    {
        public readonly ConsumedMessageBase Message;
        public readonly IOutboundChannel OutboundChannel;

        public ConsumingContext(ConsumedMessageBase message,
                                IOutboundChannel outboundChannel)
        {
            Message = message;
            OutboundChannel = outboundChannel;
        }

        internal ConsumingContext<TMessage> To<TMessage>()
            where TMessage : class
        {
            var consumedMessage = Message.To<TMessage>();
            return new ConsumingContext<TMessage>(consumedMessage, OutboundChannel);
        }
    }


    public class ConsumingContext<TMessage>
        where TMessage : class
    {
        public readonly ConsumedMessage<TMessage> Message;
        public readonly IOutboundChannel OutboundChannel;

        public ConsumingContext(ConsumedMessage<TMessage> message,
                                IOutboundChannel outboundChannel)
        {
            Message = message;
            OutboundChannel = outboundChannel;
        }
    }
}