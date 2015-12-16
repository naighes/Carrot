using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public abstract class ConsumedMessageBase
    {
        internal readonly BasicDeliverEventArgs Args;

        protected internal ConsumedMessageBase(BasicDeliverEventArgs args)
        {
            Args = args;
        }

        internal abstract Object Content { get; }

        internal abstract Task<AggregateConsumingResult> ConsumeAsync(IEnumerable<IConsumer> subscriptions);

        internal ConsumedMessage<TMessage> As<TMessage>() where TMessage : class
        {
            var content = Content as TMessage;

            if (content == null)
                throw new InvalidCastException($"cannot cast '{Content.GetType()}' to '{typeof(TMessage)}'");

            return new ConsumedMessage<TMessage>(content,
                                                 HeaderCollection.Parse(Args.BasicProperties),
                                                 Args.ConsumerTag);
        }

        internal abstract Boolean Match(Type type);

        internal void Acknowledge(IInboundChannel channel)
        {
            channel.Acknowledge(Args.DeliveryTag);
        }

        internal void Requeue(IInboundChannel inboundChannel)
        {
            inboundChannel.NegativeAcknowledge(Args.DeliveryTag, true);
        }
    }
}