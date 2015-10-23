using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AtMostOnceConsumer : ConsumerBase
    {
        internal AtMostOnceConsumer(IModel model,
                                    IConsumedMessageBuilder builder,
                                    SubscriptionConfiguration configuration)
            : base(model, builder, configuration)
        {
        }

        protected override Task ConsumeInternal(BasicDeliverEventArgs args)
        {
            Model.BasicAck(args.DeliveryTag, false);

            return ConsumeAsync(args);
        }
    }
}