using System;
using Carrot.Configuration;
using Carrot.Logging;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public abstract class ConsumingPromise
    {
        protected readonly IConsumedMessageBuilder Builder;
        protected readonly ConsumingConfiguration Configuration;
        protected readonly Func<ILog> LogBuilder;

        private readonly Queue _queue;

        protected internal ConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            ConsumingConfiguration configuration,
                                            Func<ILog> logBuilder)
        {
            _queue = queue;
            Builder = builder;
            Configuration = configuration;
            LogBuilder = logBuilder;
        }

        internal ConsumerBase Declare(IModel model)
        {
            var consumer = BuildConsumer(model);
            model.BasicConsume(_queue.Name, false, consumer);
            return consumer;
        }

        protected internal abstract ConsumerBase BuildConsumer(IModel model);
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

        protected internal override ConsumerBase BuildConsumer(IModel model)
        {
            return new LoggedAtMostOnceConsumer(model, Builder, Configuration, LogBuilder());
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

        protected internal override ConsumerBase BuildConsumer(IModel model)
        {
            return new LoggedAtLeastOnceConsumer(model, Builder, Configuration, LogBuilder());
        }
    }
}