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
                                    Queue queue,
                                    IConsumedMessageBuilder builder,
                                    ConsumingConfiguration configuration)
            : base(model, queue, builder, configuration)
        {
        }

        protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
        {
            Model.BasicAck(args.DeliveryTag, false);

            return ConsumeAsync(args);
        }
    }
}