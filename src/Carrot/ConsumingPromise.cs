using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public abstract class ConsumingPromise
    {
        private readonly MessageQueue _queue;
        private readonly IConsumedMessageBuilder _builder;
        private readonly SubscriptionConfiguration _configuration;

        internal ConsumingPromise(MessageQueue queue,
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

        protected abstract ConsumerBase BuildConsumer(IModel model,
                                                      IConsumedMessageBuilder builder,
                                                      SubscriptionConfiguration configuration);
    }

    internal class AtMostOnceConsumingPromise : ConsumingPromise
    {
        internal AtMostOnceConsumingPromise(MessageQueue queue,
                                            IConsumedMessageBuilder builder,
                                            SubscriptionConfiguration configuration)
            : base(queue, builder, configuration)
        {
        }

        protected override ConsumerBase BuildConsumer(IModel model,
                                                      IConsumedMessageBuilder builder,
                                                      SubscriptionConfiguration configuration)
        {
            return new AtMostOnceConsumer(model, builder, configuration);
        }
    }

    internal class AtLeastOnceConsumingPromise : ConsumingPromise
    {
        internal AtLeastOnceConsumingPromise(MessageQueue queue,
                                             IConsumedMessageBuilder builder,
                                             SubscriptionConfiguration configuration)
            : base(queue, builder, configuration)
        {
        }

        protected override ConsumerBase BuildConsumer(IModel model,
                                                      IConsumedMessageBuilder builder,
                                                      SubscriptionConfiguration configuration)
        {
            return new AtLeastOnceConsumer(model, builder, configuration);
        }
    }
}