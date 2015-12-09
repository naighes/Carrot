using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class ReliableOutboundChannel : OutboundChannel
    {
        private readonly ConcurrentDictionary<UInt64, Tuple<TaskCompletionSource<Boolean>, IMessage>> _confirms =
            new ConcurrentDictionary<UInt64, Tuple<TaskCompletionSource<Boolean>, IMessage>>();

        private readonly NotConfirmedMessageHandler _notConfirmedMessageHandler;

        internal ReliableOutboundChannel(IModel model,
                                         EnvironmentConfiguration configuration,
                                         IDateTimeProvider dateTimeProvider,
                                         NotConfirmedMessageHandler notConfirmedMessageHandler)
            : base(model, configuration, dateTimeProvider)
        {
            _notConfirmedMessageHandler = notConfirmedMessageHandler;
            model.ConfirmSelect(); // TODO: not here! it issues a RPC call.
            Model.BasicAcks += OnModelBasicAcks;
            Model.BasicNacks += OnModelBasicNacks;
        }

        public override Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source,
                                                                    Exchange exchange,
                                                                    String routingKey)
        {
            var properties = source.BuildBasicProperties(Configuration.MessageTypeResolver,
                                                         DateTimeProvider,
                                                         Configuration.IdGenerator);
            var body = properties.CreateEncoding()
                                 .GetBytes(properties.CreateSerializer(Configuration.SerializationConfiguration)
                                                     .Serialize(source.Content));
            var tag = Model.NextPublishSeqNo;
            var tcs = BuildTaskCompletionSource(properties);
            _confirms.TryAdd(tag, new Tuple<TaskCompletionSource<Boolean>, IMessage>(tcs, source));

            try
            {
                Model.BasicPublish(exchange.Name,
                                   routingKey,
                                   false,
                                   false,
                                   properties,
                                   body);
            }
            catch (Exception exception)
            {
                Tuple<TaskCompletionSource<Boolean>, IMessage> tuple;
                _confirms.TryRemove(tag, out tuple);
                tcs.TrySetException(exception);
            }

            return tcs.Task.ContinueWith(Result);
        }

        protected override void OnModelDisposing()
        {
            base.OnModelDisposing();

            Model.WaitForConfirms(TimeSpan.FromSeconds(30d)); // TODO: timeout should not be hardcodeds
            Model.BasicAcks -= OnModelBasicAcks;
            Model.BasicNacks -= OnModelBasicNacks;
        }

        protected virtual void OnModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            HandleServerResponse(args.DeliveryTag,
                                 args.Multiple,
                                 (_, source) => _.TrySetException(new NegativeAcknowledgeException(source, "publish was NACK-ed")));
        }

        protected virtual void OnModelBasicAcks(Object sender, BasicAckEventArgs args)
        {
            HandleServerResponse(args.DeliveryTag,
                                 args.Multiple,
                                 (_, source) => _.TrySetResult(true));
        }

        protected override void OnModelShutdown(Object sender, ShutdownEventArgs args)
        {
            base.OnModelShutdown(sender, args);

            foreach (var confirm in _confirms)
            {
                var exception = new MessageNotConfirmedException(confirm.Value.Item2,
                                                                 "publish not confirmed before channel closed");
                _notConfirmedMessageHandler?.Invoke(exception.SourceMessage);
                confirm.Value.Item1.TrySetException(exception);
            }
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

    public delegate void NotConfirmedMessageHandler(IMessage message);
}