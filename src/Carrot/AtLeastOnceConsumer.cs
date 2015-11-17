using System;
using System.Threading.Tasks;
using Carrot.Configuration;
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

            _log.Info(String.Format("consumer-model consumer.cancelled received (consumer-tag: '{0}')", // TODO: wrong name
                                    args.ConsumerTag));
        }
    }

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
            return ConsumeAsync(args).ContinueWith(_ => _.Result.Reply(Model));
        }
    }
}