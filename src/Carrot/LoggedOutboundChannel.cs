using System;
using Carrot.Configuration;
using Carrot.Logging;
using RabbitMQ.Client;

namespace Carrot
{
    internal class LoggedOutboundChannel : OutboundChannel
    {
        public LoggedOutboundChannel(IModel model,
                                     EnvironmentConfiguration configuration,
                                     IDateTimeProvider dateTimeProvider)
            : base(model, configuration, dateTimeProvider)
        {
        }

        protected override void OnModelShutdown(Object sender, ShutdownEventArgs args)
        {
            base.OnModelShutdown(sender, args);

            Log().Fatal($"outbound-model basic.nack received (reply-text: {args.ReplyText}, reply-code: {args.ReplyCode})");
        }

        private ILog Log()
        {
            return Configuration.Log;
        }
    }
}