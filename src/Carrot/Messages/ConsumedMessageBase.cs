using System;
using System.Threading.Tasks;
using Carrot.Configuration;
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

        internal abstract Task<AggregateConsumingResult> ConsumeAsync(SubscriptionConfiguration configuration);

        internal ConsumedMessage<TMessage> As<TMessage>() where TMessage : class
        {
            var content = Content as TMessage;

            if (content == null)
                throw new InvalidCastException(String.Format("cannot cast '{0}' to '{1}'",
                                                             Content.GetType(),
                                                             typeof(TMessage)));

            return new ConsumedMessage<TMessage>(content, HeaderCollection.Parse(Args));
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

        internal void ForwardTo(IModel model, Func<String, String> exchangeNameBuilder)
        {
            var exchange = Exchange.DurableDirect(exchangeNameBuilder(Args.Exchange));
            exchange.Declare(model);
            model.BasicPublish(exchange.Name,
                               String.Empty,
                               Args.BasicProperties,
                               Args.Body);
        }
    }
}