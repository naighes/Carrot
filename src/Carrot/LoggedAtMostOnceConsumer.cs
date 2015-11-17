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
    public class LoggedAtMostOnceConsumer : AtMostOnceConsumer
    {
        private readonly ILog _log;

        internal LoggedAtMostOnceConsumer(IModel model,
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

            _log.Info(String.Format("consumer-model basic.ack received (delivery-tag: {0}, multiple: {1})",
                                    args.DeliveryTag,
                                    args.Multiple));
        }

        protected override void OnModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            base.OnModelBasicNacks(sender, args);

            _log.Info(String.Format("consumer-model basic.nack received (delivery-tag: {0}, multiple: {1})",
                                    args.DeliveryTag,
                                    args.Multiple));
        }

        protected override void OnModelBasicReturn(Object sender, BasicReturnEventArgs args)
        {
            base.OnModelBasicReturn(sender, args);

            _log.Info(String.Format("consumer-model basic.return received (reply-text: '{0}', reply-code: {1})",
                                    args.ReplyText,
                                    args.ReplyCode));
        }

        protected override void OnConsumerCancelled(Object sender, ConsumerEventArgs args)
        {
            base.OnConsumerCancelled(sender, args);

            _log.Info(String.Format("consumer-model basic.cancel received (consumer-tag: '{0}')",
                                    args.ConsumerTag));
        }
    }
}