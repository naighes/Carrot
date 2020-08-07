using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AtMostOnceConsumer : ConsumerBase
    {
        internal AtMostOnceConsumer(IInboundChannel inboundChannel,
                                    IOutboundChannel outboundChannel,
                                    Queue queue,
                                    IConsumedMessageBuilder builder,
                                    ConsumingConfiguration configuration)
            : base(inboundChannel,
                   outboundChannel,
                   queue,
                   builder,
                   configuration)
        {
        }

        protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
        {
            InboundChannel.Acknowledge(args.DeliveryTag);

            return ConsumeAsync(args,
                                OutboundChannel).ContinueWith(_ =>
                                                              {
                                                                  var result = _.Result;
                                                                  result.NotifyConsumingCompletion();
                                                                  return result;
                                                              }, TaskContinuationOptions.RunContinuationsAsynchronously);
        }
    }
}