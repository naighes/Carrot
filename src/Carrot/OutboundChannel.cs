using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class OutboundChannel : IDisposable
    {
        private readonly IModel _model;

        private readonly ConcurrentDictionary<UInt64, Tuple<TaskCompletionSource<Boolean>, IMessage>> _confirms =
            new ConcurrentDictionary<UInt64, Tuple<TaskCompletionSource<Boolean>, IMessage>>();

        public OutboundChannel(IModel model)
        {
            _model = model;

            _model.BasicAcks += OnModelBasicAcks;
            _model.BasicNacks += OnModelBasicNacks;
            _model.ModelShutdown += OnModelShutdown;
        }

        public void Dispose()
        {
            if (_model == null)
                return;

            _model.WaitForConfirms(TimeSpan.FromSeconds(30d)); // TODO: timeout should not be hardcodeds

            _model.BasicAcks -= OnModelBasicAcks;
            _model.BasicNacks -= OnModelBasicNacks;
            _model.ModelShutdown -= OnModelShutdown;

            _model.Dispose();
        }

        internal OutboundMessageEnvelope<TMessage> BuildEnvelope<TMessage>(IBasicProperties properties,
                                                                           Byte[] body,
                                                                           Exchange exchange,
                                                                           String routingKey,
                                                                           OutboundMessage<TMessage> source)
            where TMessage : class
        {
            var tag = _model.NextPublishSeqNo;
            return new OutboundMessageEnvelope<TMessage>(properties, body, exchange, routingKey, tag, source);
        }

        internal Task<IPublishResult> PublishAsync<TMessage>(OutboundMessageEnvelope<TMessage> message)
            where TMessage : class
        {
            var tcs = new TaskCompletionSource<Boolean>(message.Properties);
            _confirms.TryAdd(message.Tag,
                             new Tuple<TaskCompletionSource<Boolean>, IMessage>(tcs, message.Source));

            try
            {
                _model.BasicPublish(message.Exchange.Name,
                                    message.RoutingKey,
                                    false,
                                    false,
                                    message.Properties,
                                    message.Body);
            }
            catch (Exception exception)
            {
                Tuple<TaskCompletionSource<Boolean>, IMessage> tuple;
                _confirms.TryRemove(message.Tag, out tuple);
                tcs.TrySetException(exception);
            }

            return tcs.Task.ContinueWith(Result);
        }

        protected virtual void OnModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            HandleServerResponse(args.DeliveryTag,
                                 args.Multiple,
                                 (_, source) => _.TrySetException(new NegativeAckReceivedException(source,
                                                                                                   "publish was NACK-ed")));
        }

        protected virtual void OnModelBasicAcks(Object sender, BasicAckEventArgs args)
        {
            HandleServerResponse(args.DeliveryTag,
                                 args.Multiple,
                                 (_, source) => _.TrySetResult(true));
        }

        protected virtual void OnModelShutdown(Object sender, ShutdownEventArgs args)
        {
            foreach (var confirm in _confirms)
            {
                var exception = new MessageNotConfirmedException(confirm.Value.Item2,
                                                                 "publish not confirmed before channel closed");
                confirm.Value.Item1.TrySetException(exception);
            }
        }

        private static IPublishResult Result(Task task)
        {
            if (task.Exception != null)
                return new FailurePublishing(task.Exception.GetBaseException());

            return SuccessfulPublishing.FromBasicProperties(task.AsyncState as IBasicProperties);
        }

        private void HandleServerResponse(UInt64 deliveryTag,
                                          Boolean multiple,
                                          Action<TaskCompletionSource<Boolean>, IMessage> action)
        {
            var tags = multiple
                ? _confirms.Keys.Where(_ => _ <= deliveryTag)
                : Enumerable.Repeat(deliveryTag, 1);

            foreach (var tag in tags)
            {
                var confirm = _confirms[tag];
                action(confirm.Item1, confirm.Item2);
                Tuple<TaskCompletionSource<Boolean>, IMessage> tuple;
                _confirms.TryRemove(tag, out tuple);
            }
        }
    }

    public class NegativeAckReceivedException : Exception
    {
        internal NegativeAckReceivedException(IMessage source, String message)
            : base(message)
        {
            SourceMessage = source;
        }

        public IMessage SourceMessage { get; }
    }

    public class MessageNotConfirmedException : Exception
    {
        internal MessageNotConfirmedException(IMessage source, String message)
            : base(message)
        {
            SourceMessage = source;
        }

        public IMessage SourceMessage { get; }
    }
}