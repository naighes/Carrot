using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Logging;
using Carrot.Messages;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class LoggedAtMostOnceConsumer : AtMostOnceConsumer
    {
        private readonly ILog _log;

        internal LoggedAtMostOnceConsumer(IInboundChannel inboundChannel,
                                          IOutboundChannel outboundChannel,
                                          Queue queue,
                                          IConsumedMessageBuilder builder,
                                          ConsumingConfiguration configuration,
                                          ILog log)
            : base(inboundChannel, outboundChannel, queue, builder, configuration)
        {
            _log = log;
        }

        protected internal override Task<AggregateConsumingResult> ConsumeAsync(BasicDeliverEventArgs args)
        {
            return base.ConsumeAsync(args).ContinueWith(_ => _.HandleErrorResult(_log));
        }

        protected override void OnConsumerCancelled(Object sender, ConsumerEventArgs args)
        {
            base.OnConsumerCancelled(sender, args);

            _log.Info($"consumer-model basic.cancel received (consumer-tag: '{args.ConsumerTag}')");
        }
    }
}