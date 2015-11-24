using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Logging;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class LoggedAtLeastOnceConsumer : AtLeastOnceConsumer
    {
        private readonly ILog _log;

        internal LoggedAtLeastOnceConsumer(IModel model,
                                           IConsumedMessageBuilder builder,
                                           ConsumingConfiguration configuration,
                                           ILog log)
            : base(model, builder, configuration)
        {
            _log = log;
        }

        protected internal override Task<AggregateConsumingResult> ConsumeAsync(BasicDeliverEventArgs args)
        {
            return base.ConsumeAsync(args).ContinueWith(_ => _.HandleErrorResult(_log));
        }

        protected override void OnModelBasicAcks(Object sender, BasicAckEventArgs args)
        {
            base.OnModelBasicAcks(sender, args);

            _log.Info($"consumer-model basic.ack received (delivery-tag: {args.DeliveryTag}, multiple: {args.Multiple})");
        }

        protected override void OnModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            base.OnModelBasicNacks(sender, args);

            _log.Info($"consumer-model basic.nack received (delivery-tag: {args.DeliveryTag}, multiple: {args.Multiple})");
        }

        protected override void OnModelBasicReturn(Object sender, BasicReturnEventArgs args)
        {
            base.OnModelBasicReturn(sender, args);

            _log.Info($"consumer-model basic.return received (reply-text: '{args.ReplyText}', reply-code: {args.ReplyCode})");
        }

        protected override void OnConsumerCancelled(Object sender, ConsumerEventArgs args)
        {
            base.OnConsumerCancelled(sender, args);

            _log.Info($"consumer-model basic.cancel received (consumer-tag: '{args.ConsumerTag}')");
        }
    }
}