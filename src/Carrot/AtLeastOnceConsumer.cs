using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AtLeastOnceConsumer : ConsumerBase
    {
        internal AtLeastOnceConsumer(IInboundChannel inboundChannel,
                                     IOutboundChannelPool outboundChannelPool,
                                     Queue queue,
                                     IConsumedMessageBuilder builder,
                                     ConsumingConfiguration configuration)
            : base(inboundChannel, outboundChannelPool, queue, builder, configuration)
        {
        }

        protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
        {
            return ConsumeAsync(args).ContinueWith(_ =>
            {
                var result = _.Result;
                try
                {
                    result.Reply(InboundChannel,
                        OutboundChannelPool,
                        Configuration.FallbackStrategy);
                    result.NotifyConsumingCompletion();
                }
                catch (Exception e)
                {
                    result.NotifyConsumingFault(e);
                }
                return result;
            });
        }
    }
}