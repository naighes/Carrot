using System;
using Carrot.Configuration;
using Carrot.Logging;
using Carrot.Messages;

namespace Carrot
{
    public abstract class ConsumingPromise
    {
        protected readonly Queue Queue;
        protected readonly IConsumedMessageBuilder Builder;
        protected readonly ConsumingConfiguration Configuration;
        protected readonly Func<ILog> LogBuilder;

        protected internal ConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            ConsumingConfiguration configuration,
                                            Func<ILog> logBuilder)
        {
            Queue = queue;
            Builder = builder;
            Configuration = configuration;
            LogBuilder = logBuilder;
        }

        internal abstract ConsumerBase BuildConsumer(IInboundChannel inboundChannel, IOutboundChannel outboundChannel);
    }

    internal class AtMostOnceConsumingPromise : ConsumingPromise
    {
        internal AtMostOnceConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            ConsumingConfiguration configuration,
                                            Func<ILog> logBuilder)
            : base(queue, builder, configuration, logBuilder)
        {
        }

        internal override ConsumerBase BuildConsumer(IInboundChannel inboundChannel, IOutboundChannel outboundChannel)
        {
            return new LoggedAtMostOnceConsumer(inboundChannel,
                                                outboundChannel,
                                                Queue,
                                                Builder,
                                                Configuration,
                                                LogBuilder());
        }
    }

    internal class AtLeastOnceConsumingPromise : ConsumingPromise
    {
        internal AtLeastOnceConsumingPromise(Queue queue,
                                             IConsumedMessageBuilder builder,
                                             ConsumingConfiguration configuration,
                                             Func<ILog> logBuilder)
            : base(queue, builder, configuration, logBuilder)
        {
        }

        internal override ConsumerBase BuildConsumer(IInboundChannel inboundChannel, IOutboundChannel outboundChannel)
        {
            return new LoggedAtLeastOnceConsumer(inboundChannel,
                                                 outboundChannel,
                                                 Queue,
                                                 Builder,
                                                 Configuration,
                                                 LogBuilder());
        }
    }
}