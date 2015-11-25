using System;
using System.Collections.Generic;
using System.Linq;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class Channel : IChannel
    {
        private readonly ChannelConfiguration _configuration;

        private readonly ISet<Queue> _queues = new HashSet<Queue>();
        private readonly ISet<Exchange> _exchanges = new HashSet<Exchange>();
        private readonly ISet<ExchangeBinding> _bindings = new HashSet<ExchangeBinding>();
        private readonly ISet<Func<IConsumedMessageBuilder, ConsumingPromise>> _promises = new HashSet<Func<IConsumedMessageBuilder, ConsumingPromise>>();

        protected internal Channel(ChannelConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static IChannel New(Action<ChannelConfiguration> configure)
        {
            var configuration = new ChannelConfiguration();
            configure(configuration);
            return new Channel(configuration);
        }

        public Queue DeclareQueue(String name)
        {
            return DeclareQueue(name, false);
        }

        public Queue DeclareDurableQueue(String name)
        {
            return DeclareQueue(name, true);
        }

        public Exchange DeclareDirectExchange(String name)
        {
            return DeclareExchange(name, "direct", false);
        }

        public Exchange DeclareDurableDirectExchange(String name)
        {
            return DeclareExchange(name, "direct", true);
        }

        public Exchange DeclareFanoutExchange(String name)
        {
            return DeclareExchange(name, "fanout", false);
        }

        public Exchange DeclareDurableFanoutExchange(String name)
        {
            return DeclareExchange(name, "fanout", true);
        }

        public Exchange DeclareTopicExchange(String name)
        {
            return DeclareExchange(name, "topic", false);
        }

        public Exchange DeclareDurableTopicExchange(String name)
        {
            return DeclareExchange(name, "topic", true);
        }

        public Exchange DeclareHeadersExchange(String name)
        {
            return DeclareExchange(name, "headers", false);
        }

        public Exchange DeclareDurableHeadersExchange(String name)
        {
            return DeclareExchange(name, "headers", true);
        }

        public void DeclareExchangeBinding(Exchange exchange, Queue queue, String routingKey = "")
        {
            if (exchange == null)
                throw new ArgumentNullException(nameof(exchange));

            if (queue == null)
                throw new ArgumentNullException(nameof(queue));

            if (routingKey == null)
                throw new ArgumentNullException(nameof(routingKey));

            if (!_bindings.Add(new ExchangeBinding(exchange, queue, routingKey)))
                throw new ArgumentException("dupicate binding detected");
        }

        public IAmqpConnection Connect()
        {
            var connection = CreateConnection();
            var outboundModel = connection.CreateModel();

            foreach (var exchange in _exchanges)
                exchange.Declare(outboundModel);

            foreach (var queue in _queues)
                queue.Declare(outboundModel);

            foreach (var binding in _bindings)
                binding.Declare(outboundModel);

            var builder = new ConsumedMessageBuilder(_configuration.SerializationConfiguration,
                                                     _configuration.MessageTypeResolver);

            return new AmqpConnection(connection,
                                      _promises.Select(_ => BuildConsumer(connection, _, builder)).ToList(),
                                      new LoggedReliableOutboundChannel(outboundModel, _configuration),
                                      new DateTimeProvider(),
                                      _configuration);
        }

        public void SubscribeByAtMostOnce(Queue queue, Action<ConsumingConfiguration> configure)
        {
            Subscribe(configure,
                      (b, c) => new AtMostOnceConsumingPromise(queue, b, c, () => _configuration.Log),
                      queue);
        }

        public void SubscribeByAtLeastOnce(Queue queue, Action<ConsumingConfiguration> configure)
        {
            Subscribe(configure,
                      (b, c) => new AtLeastOnceConsumingPromise(queue, b, c, () => _configuration.Log),
                      queue);
        }

        protected internal virtual IConnection CreateConnection()
        {
            return new ConnectionFactory
                       {
                           Uri = _configuration.EndpointUri.ToString(),
                           AutomaticRecoveryEnabled = true,
                           TopologyRecoveryEnabled = true
                       }.CreateConnection();
        }

        protected internal virtual IModel CreateInboundModel(IConnection connection,
                                                             UInt32 prefetchSize,
                                                             UInt16 prefetchCount)
        {
            var model = connection.CreateModel();
            model.BasicQos(prefetchSize, prefetchCount, false);
            return model;
        }

        private ConsumerBase BuildConsumer(IConnection connection,
                                           Func<IConsumedMessageBuilder, ConsumingPromise> promise,
                                           IConsumedMessageBuilder builder)
        {
            return promise(builder).Declare(CreateInboundModel(connection,
                                                               _configuration.PrefetchSize,
                                                               _configuration.PrefetchCount));
        }

        private void Subscribe(Action<ConsumingConfiguration> configure,
                               Func<IConsumedMessageBuilder, ConsumingConfiguration, ConsumingPromise> func,
                               Queue queue)
        {
            var configuration = new ConsumingConfiguration(this, queue);
            configure(configuration);
            Func<IConsumedMessageBuilder, ConsumingPromise> f = _ => func(_, configuration);
            _promises.Add(f);
        }

        private Queue DeclareQueue(String name, Boolean isDurable)
        {
            var queue = new Queue(name, isDurable);
            _queues.Add(queue);
            return queue;
        }

        private Exchange DeclareExchange(String name, String type, Boolean isDurable)
        {
            var exchange = new Exchange(name, type, isDurable);
            _exchanges.Add(exchange);
            return exchange;
        }
    }
}