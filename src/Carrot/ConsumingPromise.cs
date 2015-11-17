using System;
using Carrot.Configuration;
using Carrot.Logging;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public abstract class ConsumingPromise
    {
        protected readonly Func<ILog> LogBuilder;

        private readonly Queue _queue;
        private readonly IConsumedMessageBuilder _builder;
        private readonly ConsumingConfiguration _configuration;

        protected internal ConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            ConsumingConfiguration configuration,
                                            Func<ILog> logBuilder)
        {
            _queue = queue;
            _builder = builder;
            _configuration = configuration;
            LogBuilder = logBuilder;
        }

        internal ConsumerBase Declare(IModel model)
        {
            var consumer = BuildConsumer(model, _builder, _configuration);
            model.BasicConsume(_queue.Name, false, consumer);
            return consumer;
        }

        protected internal abstract ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               ConsumingConfiguration configuration);
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

        protected internal override ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               ConsumingConfiguration configuration)
        {
            return new LoggedAtMostOnceConsumer(model, builder, configuration, LogBuilder());
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

        protected internal override ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               ConsumingConfiguration configuration)
        {
            return new LoggedAtLeastOnceConsumer(model, builder, configuration, LogBuilder());
        }
    }
}