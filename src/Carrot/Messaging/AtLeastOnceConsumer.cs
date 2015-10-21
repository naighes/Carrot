using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messaging
{
    // TODO: plug consumer type
    public class AtLeastOnceConsumer : ConsumerBase
    {
        internal AtLeastOnceConsumer(IModel model,
                                     IConsumedMessageBuilder builder,
                                     SubscriptionConfiguration configuration)
            : base(model, builder, configuration)
        {
        }

        protected override Task ConsumeInternal(BasicDeliverEventArgs args)
        {
            return Builder.Build(args)
                          .ConsumeAsync(Configuration)
                          .ContinueWith(_ => _.Result.Reply(Model));
        }
    }
}