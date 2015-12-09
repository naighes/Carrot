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

        internal OutboundChannel(IModel model)
        {
            Model = model;
            Model.ModelShutdown += OnModelShutdown;
        }

        public static Func<IModel, EnvironmentConfiguration, IOutboundChannel> Default()
        {
            return (m, c) => new LoggedOutboundChannel(m, c);
        }

        public static Func<IModel, EnvironmentConfiguration, IOutboundChannel> Reliable(NotConfirmedMessageHandler handler = null)
        {
            return (m, c) => new LoggedReliableOutboundChannel(m, handler ?? (_ => { }), c);
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
                                                                   IBasicProperties properties,
                                                                   Byte[] body,
                                                                   Exchange exchange,
                                                                   String routingKey)
            where TMessage : class
        {
            var message = source.BuildEnvelope(Model,
                                               properties,
                                               body,
                                               exchange,
                                               routingKey);
            var tcs = BuildTaskCompletionSource(message);

            try
            {
                Model.BasicPublish(message.Exchange.Name,
                                   message.RoutingKey,
                                   false,
                                   false,
                                   message.Properties,
                                   message.Body);
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

        protected TaskCompletionSource<Boolean> BuildTaskCompletionSource<TMessage>(OutboundMessageEnvelope<TMessage> message)
            where TMessage : class
        {
            return new TaskCompletionSource<Boolean>(message.Properties);
        }

        protected virtual void OnModelShutdown(Object sender, ShutdownEventArgs args) { }

        protected virtual void OnModelDisposing() { }
    }
}