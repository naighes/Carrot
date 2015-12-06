using System;
using Carrot.Configuration;
using Carrot.Logging;
using Carrot.Messages;
using RabbitMQ.Client;

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

        internal abstract ConsumerBase BuildConsumer(IModel model);
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

        internal override ConsumerBase BuildConsumer(IModel model)
        {
            return new LoggedAtMostOnceConsumer(model, Queue, Builder, Configuration, LogBuilder());
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

        internal override ConsumerBase BuildConsumer(IModel model)
        {
            return new LoggedAtLeastOnceConsumer(model, Queue, Builder, Configuration, LogBuilder());
        }
    }
}