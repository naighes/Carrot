using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public abstract class ConsumingPromise
    {
        private readonly Queue _queue;
        private readonly IConsumedMessageBuilder _builder;
        private readonly ConsumingConfiguration _configuration;

        protected internal ConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            ConsumingConfiguration configuration)
        {
            _queue = queue;
            _builder = builder;
            _configuration = configuration;
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
                                            ConsumingConfiguration configuration)
            : base(queue, builder, configuration)
        {
        }

        protected internal override ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               ConsumingConfiguration configuration)
        {
            return new AtMostOnceConsumer(model, builder, configuration);
        }
    }

    internal class AtLeastOnceConsumingPromise : ConsumingPromise
    {
        internal AtLeastOnceConsumingPromise(Queue queue,
                                             IConsumedMessageBuilder builder,
                                             ConsumingConfiguration configuration)
            : base(queue, builder, configuration)
        {
        }

        protected internal override ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               ConsumingConfiguration configuration)
        {
            return new AtLeastOnceConsumer(model, builder, configuration);
        }
    }
}