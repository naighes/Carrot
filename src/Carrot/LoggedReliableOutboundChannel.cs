using System;
using Carrot.Configuration;
using Carrot.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    internal class LoggedReliableOutboundChannel : ReliableOutboundChannel
    {
        private readonly ChannelConfiguration _configuration;

        internal LoggedReliableOutboundChannel(IModel model, ChannelConfiguration configuration)
            : base(model)
        {
            _configuration = configuration;
        }

        protected override void OnModelBasicAcks(Object sender, BasicAckEventArgs args)
        {
            base.OnModelBasicAcks(sender, args);

            Log().Info($"outbound-model basic.ack received (delivery-tag: {args.DeliveryTag}, multiple: {args.Multiple})");
        }

        protected override void OnModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            base.OnModelBasicNacks(sender, args);

            Log().Info($"outbound-model basic.nack received (delivery-tag: {args.DeliveryTag}, multiple: {args.Multiple})");
        }

        protected override void OnModelShutdown(Object sender, ShutdownEventArgs args)
        {
            base.OnModelShutdown(sender, args);

            Log().Fatal($"outbound-model basic.nack received (reply-text: {args.ReplyText}, reply-code: {args.ReplyCode})");
        }

        private ILog Log()
        {
            return _configuration.Log;
        }
    }
}