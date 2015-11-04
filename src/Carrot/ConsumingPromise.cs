using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public abstract class ConsumingPromise
    {
        private readonly Queue _queue;
        private readonly IConsumedMessageBuilder _builder;
        private readonly SubscriptionConfiguration _configuration;

        protected internal ConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            SubscriptionConfiguration configuration)
        {
            _queue = queue;
            _builder = builder;
            _configuration = configuration;
        }

        internal void Declare(IModel model)
        {
            var consumer = BuildConsumer(model, _builder, _configuration);
            model.BasicConsume(_queue.Name, false, consumer);
        }

        protected internal abstract ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               SubscriptionConfiguration configuration);
    }

    internal class AtMostOnceConsumingPromise : ConsumingPromise
    {
        internal AtMostOnceConsumingPromise(Queue queue,
                                            IConsumedMessageBuilder builder,
                                            SubscriptionConfiguration configuration)
            : base(queue, builder, configuration)
        {
        }

        protected internal override ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               SubscriptionConfiguration configuration)
        {
            return new AtMostOnceConsumer(model, builder, configuration);
        }
    }

    internal class AtLeastOnceConsumingPromise : ConsumingPromise
    {
        internal AtLeastOnceConsumingPromise(Queue queue,
                                             IConsumedMessageBuilder builder,
                                             SubscriptionConfiguration configuration)
            : base(queue, builder, configuration)
        {
        }

        protected internal override ConsumerBase BuildConsumer(IModel model,
                                                               IConsumedMessageBuilder builder,
                                                               SubscriptionConfiguration configuration)
        {
            return new AtLeastOnceConsumer(model, builder, configuration);
        }
    }
}