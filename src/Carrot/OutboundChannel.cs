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

        public virtual Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source,
                                                                   Exchange exchange,
                                                                   String routingKey)
            where TMessage : class
        {
            var properties = source.BuildBasicProperties(Configuration.MessageTypeResolver,
                                                         DateTimeProvider,
                                                         Configuration.IdGenerator);
            var body = properties.CreateEncoding()
                                 .GetBytes(properties.CreateSerializer(Configuration.SerializationConfiguration)
                                                     .Serialize(source.Content));
            var tcs = BuildTaskCompletionSource(properties);

            try
            {
                Model.BasicPublish(exchange.Name,
                                   routingKey,
                                   false,
                                   false,
                                   properties,
                                   body);
                tcs.TrySetResult(true);
            }
            catch (Exception exception) { tcs.TrySetException(exception); }

            return tcs.Task.ContinueWith(Result);
        }

        protected static IPublishResult Result(Task task)
        {
            if (task.Exception != null)
                return new FailurePublishing(task.Exception.GetBaseException());

            return SuccessfulPublishing.FromBasicProperties(task.AsyncState as IBasicProperties);
        }

        protected TaskCompletionSource<Boolean> BuildTaskCompletionSource(IBasicProperties properties)
        {
            return new TaskCompletionSource<Boolean>(properties);
        }

        protected virtual void OnModelShutdown(Object sender, ShutdownEventArgs args) { }

        protected virtual void OnModelDisposing() { }
    }
}