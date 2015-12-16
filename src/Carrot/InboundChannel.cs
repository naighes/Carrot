using System;
using Carrot.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public interface IInboundChannel : IDisposable
    {
        void Acknowledge(UInt64 deliveryTag);

        void NegativeAcknowledge(UInt64 deliveryTag, Boolean requeue);
    }

    public class LoggedInboundChannel : InboundChannel
    {
        private readonly ILog _log;

        public LoggedInboundChannel(IModel model, ILog log)
            : base(model)
        {
            _log = log;
        }

        protected override void OnModelBasicAcks(Object sender, BasicAckEventArgs args)
        {
            base.OnModelBasicAcks(sender, args);

            _log.Info($"consumer-model basic.ack received (delivery-tag: {args.DeliveryTag}, multiple: {args.Multiple})");
        }

        protected override void OnModelBasicNacks(Object sender, BasicNackEventArgs args)
        {
            base.OnModelBasicNacks(sender, args);

            _log.Info($"consumer-model basic.nack received (delivery-tag: {args.DeliveryTag}, multiple: {args.Multiple})");
        }

        protected override void OnModelBasicReturn(Object sender, BasicReturnEventArgs args)
        {
            base.OnModelBasicReturn(sender, args);

            _log.Info($"consumer-model basic.return received (reply-text: '{args.ReplyText}', reply-code: {args.ReplyCode})");
        }
    }

    public class InboundChannel : IInboundChannel
    {
        private readonly IModel _model;

        public InboundChannel(IModel model)
        {
            _model = model;

            _model.BasicAcks += OnModelBasicAcks;
            _model.BasicNacks += OnModelBasicNacks;
            _model.BasicReturn += OnModelBasicReturn;
        }

        public void Acknowledge(UInt64 deliveryTag)
        {
            _model.BasicAck(deliveryTag, false);
        }

        public void NegativeAcknowledge(UInt64 deliveryTag, Boolean requeue)
        {
            _model.BasicNack(deliveryTag, false, requeue);
        }
        
        public void Dispose()
        {
            if (_model == null)
                return;

            _model.BasicAcks -= OnModelBasicAcks;
            _model.BasicNacks -= OnModelBasicNacks;
            _model.BasicReturn -= OnModelBasicReturn;
            _model.Dispose();
        }

        protected virtual void OnModelBasicReturn(Object sender, BasicReturnEventArgs args) { }

        protected virtual void OnModelBasicNacks(Object sender, BasicNackEventArgs args) { }

        protected virtual void OnModelBasicAcks(Object sender, BasicAckEventArgs args) { }
    }
}