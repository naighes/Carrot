using System;
using RabbitMQ.Client;

namespace Carrot
{
    public interface IInboundChannel : IDisposable
    {
        void Acknowledge(UInt64 deliveryTag);

        void NegativeAcknowledge(UInt64 deliveryTag, Boolean requeue);
    }

    public class InboundChannel : IInboundChannel
    {
        private readonly IModel _model;

        public InboundChannel(IModel model)
        {
            _model = model;
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
        }
    }
}