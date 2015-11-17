using System;
using System.Collections.Generic;
using Carrot.Configuration;
using Carrot.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class LoggedAmqpConnection : AmqpConnection
    {
        internal LoggedAmqpConnection(IConnection connection,
                                      IEnumerable<ConsumerBase> consumers,
                                      IModel outboundModel,
                                      IDateTimeProvider dateTimeProvider,
                                      ChannelConfiguration configuration)
            : base(connection, consumers, outboundModel, dateTimeProvider, configuration)
        {
        }

        protected override void OnOutboundModelBasicAcks(Object sender, BasicAckEventArgs args)
        {
            base.OnOutboundModelBasicAcks(sender, args);

            Log().Info(String.Format("outbound-model basic.ack received (delivery-tag: {0}, multiple: {1})",
                                     args.DeliveryTag,
                                     args.Multiple));
        }

        protected override void OnOutboundModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            base.OnOutboundModelBasicNacks(sender, args);

            Log().Info(String.Format("outbound-model basic.nack received (delivery-tag: {0}, multiple: {1})",
                                     args.DeliveryTag,
                                     args.Multiple));
        }

        protected override void OnOutboundModelBasicReturn(Object sender, BasicReturnEventArgs args)
        {
            base.OnOutboundModelBasicReturn(sender, args);

            Log().Info(String.Format("outbound-model basic.return received (reply-text: '{0}', reply-code: {1})",
                                     args.ReplyText,
                                     args.ReplyCode));
        }

        private ILog Log()
        {
            return Configuration.Log;
        }
    }
}