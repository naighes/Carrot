using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public abstract class ConsumedMessageBase
    {
        protected readonly BasicDeliverEventArgs Args;

        protected ConsumedMessageBase(BasicDeliverEventArgs args)
        {
            Args = args;
        }

        internal abstract Object Content { get; }

        internal abstract Task<AggregateConsumingResult> ConsumeAsync(ConsumingConfiguration configuration);

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

        internal void Acknowledge(IModel model)
        {
            model.BasicAck(Args.DeliveryTag, false);
        }

        internal void Requeue(IModel model)
        {
            model.BasicNack(Args.DeliveryTag, false, true);
        }

        internal void PersistentForwardTo(IModel model,
                                          Exchange exchange,
                                          String routingKey = "",
                                          Boolean mandatory = false,
                                          Boolean immediate = false)
        {
            InternalForwardTo(model, exchange, routingKey, mandatory, immediate, true);
        }

        internal void ForwardTo(IModel model,
                                Exchange exchange,
                                String routingKey = "",
                                Boolean mandatory = false,
                                Boolean immediate = false)
        {
            InternalForwardTo(model, exchange, routingKey, mandatory, immediate, false);
        }

        internal void InternalForwardTo(IModel model,
                                        Exchange exchange,
                                        String routingKey = "",
                                        Boolean mandatory = false,
                                        Boolean immediate = false,
                                        Boolean persistent = false)
        {
            var properties = Args.BasicProperties.Copy();
            properties.Persistent = persistent;
            model.BasicPublish(exchange.Name,
                               routingKey,
                               mandatory,
                               immediate,
                               properties,
                               Args.Body);
        }
    }
}