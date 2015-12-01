using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AtLeastOnceConsumer : ConsumerBase
    {
        internal AtLeastOnceConsumer(IModel model,
                                     IConsumedMessageBuilder builder,
                                     ConsumingConfiguration configuration)
            : base(model, builder, configuration)
        {
        }

        protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
        {
            return ConsumeAsync(args).ContinueWith(_ => _.Result.Reply(Model, Configuration.FallbackStrategy));
        }
    }
}