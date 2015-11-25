using System;
using System.Threading.Tasks;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class OutboundChannel : IDisposable
    {
        protected readonly IModel Model;

        public OutboundChannel(IModel model)
        {
            Model = model;
            Model.ModelShutdown += OnModelShutdown;
        }

        public void Dispose()
        {
            if (Model == null)
                return;

            OnModelDisposing();
            Model.ModelShutdown -= OnModelShutdown;
            Model.Dispose();
        }

        internal OutboundMessageEnvelope<TMessage> BuildEnvelope<TMessage>(IBasicProperties properties,
                                                                           Byte[] body,
                                                                           Exchange exchange,
                                                                           String routingKey,
                                                                           OutboundMessage<TMessage> source)
            where TMessage : class
        {
            var tag = Model.NextPublishSeqNo;
            return new OutboundMessageEnvelope<TMessage>(properties,
                                                         body,
                                                         exchange,
                                                         routingKey,
                                                         tag,
                                                         source);
        }

        internal virtual Task<IPublishResult> PublishAsync<TMessage>(OutboundMessageEnvelope<TMessage> message)
            where TMessage : class
        {
            var tcs = BuildTaskCompletionSource(message);

            try
            {
                PublishInternal(message);
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

        protected void PublishInternal<TMessage>(OutboundMessageEnvelope<TMessage> message) where TMessage : class
        {
            Model.BasicPublish(message.Exchange.Name,
                               message.RoutingKey,
                               false,
                               false,
                               message.Properties,
                               message.Body);
        }

        protected virtual void OnModelShutdown(Object sender, ShutdownEventArgs args) { }

        protected virtual void OnModelDisposing() { }
    }
}