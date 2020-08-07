using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class OutboundChannel : IOutboundChannel
    {
        protected readonly IModel Model;
        protected readonly EnvironmentConfiguration Configuration;
        protected readonly IDateTimeProvider DateTimeProvider;

        internal OutboundChannel(IModel model,
                                 EnvironmentConfiguration configuration,
                                 IDateTimeProvider dateTimeProvider)
        {
            Model = model;
            Configuration = configuration;
            DateTimeProvider = dateTimeProvider;
            Model.ModelShutdown += OnModelShutdown;
        }

        public static Func<IModel, EnvironmentConfiguration, IOutboundChannel> Default()
        {
            return (m, c) => new LoggedOutboundChannel(m, c, new DateTimeProvider());
        }

        public static Func<IModel, EnvironmentConfiguration, IOutboundChannel> Reliable(NotConfirmedMessageHandler handler = null)
        {
            return (m, c) => new LoggedReliableOutboundChannel(m, c, new DateTimeProvider(), handler ?? (_ => { }));
        }

        public void Dispose()
        {
            if (Model == null)
                return;

            OnModelDisposing();
            Model.ModelShutdown -= OnModelShutdown;
            Model.Dispose();
        }



        public virtual Task<IPublishResult> ForwardAsync(ConsumedMessageBase message,
                                                         Exchange exchange,
                                                         String routingKey)
        {
            return ForwardAsync(message, exchange.Name, routingKey);
        }

        public virtual Task<IPublishResult> ForwardAsync(ConsumedMessageBase message,
                                                         String exchange,
                                                         String routingKey)
        {
            var properties = message.Args
                    .BasicProperties ?? message.Args.BasicProperties
                                                                            ;
            var body = message.Args.Body;

            return PublishInternalAsync(exchange, routingKey, properties, body);
        }

        public virtual Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source,
                                                                   Exchange exchange,
                                                                   String routingKey)
            where TMessage : class
        {
            return PublishAsync(source, exchange.Name, routingKey);
        }

        public virtual Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source,
                                                                   String exchange,
                                                                   String routingKey)
            where TMessage : class
        {
            var properties = BuildBasicProperties(source);
            var body = BuildBody(source, properties);
            return PublishInternalAsync(exchange, routingKey, properties, body);
        }

        protected static IPublishResult Result(Task task)
        {
            if (task.Exception != null)
                return new FailurePublishing(task.Exception.GetBaseException());

            return SuccessfulPublishing.FromBasicProperties(task.AsyncState as IBasicProperties);
        }

        protected IBasicProperties BuildBasicProperties<TMessage>(OutboundMessage<TMessage> source)
            where TMessage : class
        {
            return source.BuildBasicProperties(Model.CreateBasicProperties(),
                                               Configuration.MessageTypeResolver,
                                               DateTimeProvider,
                                               Configuration.IdGenerator);
        }

        protected Byte[] BuildBody(IMessage source, IBasicProperties properties)
        {
            return properties.CreateEncoding()
                             .GetBytes(properties.CreateSerializer(Configuration.SerializationConfiguration)
                             .Serialize(source.Content));
        }

        protected virtual void OnModelShutdown(Object sender, ShutdownEventArgs args) { }

        protected virtual void OnModelDisposing() { }

        private Task<IPublishResult> PublishInternalAsync(String exchange,
                                                          String routingKey,
                                                          IBasicProperties properties,
                                                          ReadOnlyMemory<Byte> body)
        {
            var tcs = new TaskCompletionSource<Boolean>(properties);

            try
            {
                Model.BasicPublish(exchange, routingKey, false, properties, body);
                tcs.TrySetResult(true);
            }
            catch (Exception exception) { tcs.TrySetException(exception); }

            return tcs.Task.ContinueWith(Result);
        }
    }
}