using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class OutboundChannel : IDisposable
    {
        public OutboundChannel(IModel model)
        {
            Model = model;

            Model.BasicAcks += OnModelBasicAcks;
            Model.BasicNacks += OnModelBasicNacks;
            Model.BasicReturn += OnModelBasicReturn;
        }

        internal IModel Model { get; }

        public void Dispose()
        {
            if (Model == null)
                return;

            Model.WaitForConfirms(TimeSpan.FromSeconds(30d)); // TODO: timeout should not be hardcodeds

            Model.BasicAcks -= OnModelBasicAcks;
            Model.BasicNacks -= OnModelBasicNacks;
            Model.BasicReturn -= OnModelBasicReturn;

            Model.Dispose();
        }

        protected virtual void OnModelBasicReturn(Object sender, BasicReturnEventArgs args) { }

        protected virtual void OnModelBasicNacks(Object sender, BasicNackEventArgs args) { }

        protected virtual void OnModelBasicAcks(Object sender, BasicAckEventArgs args) { }
    }
}