using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AtMostOnceConsumer : ConsumerBase
    {
        internal AtMostOnceConsumer(IInboundChannel inboundChannel,
                                    IOutboundChannelPool outboundChannelPool,
                                    Queue queue,
                                    IConsumedMessageBuilder builder,
                                    ConsumingConfiguration configuration)
            : base(inboundChannel, outboundChannelPool, queue, builder, configuration)
        {
        }

        protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
        {
            InboundChannel.Acknowledge(args.DeliveryTag);

            return ConsumeAsync(args).ContinueWith(_ =>
                                                   {
                                                       var result = _.Result;
                                                       result.NotifyConsumingCompletion();
                                                       return result;
                                                   });
        }
    }
}