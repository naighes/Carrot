using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class OutboundChannel : IDisposable
    {
        private readonly IModel _model;

        public OutboundChannel(IModel model)
        {
            _model = model;

            _model.BasicAcks += OnModelBasicAcks;
            _model.BasicNacks += OnModelBasicNacks;
            _model.BasicReturn += OnModelBasicReturn;
        }

        internal IModel Model => _model;

        public void Dispose()
        {
            if (_model == null)
                return;

            _model.WaitForConfirms(TimeSpan.FromSeconds(30d)); // TODO: timeout should not be hardcodeds

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