using System;
using Carrot.Configuration;
using Carrot.Logging;
using RabbitMQ.Client;

namespace Carrot
{
    internal class LoggedOutboundChannel : OutboundChannel
    {
        private readonly ChannelConfiguration _configuration;

        public LoggedOutboundChannel(IModel model, ChannelConfiguration configuration)
            : base(model)
        {
            _configuration = configuration;
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