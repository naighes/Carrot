using System;
using System.Threading.Tasks;
using Carrot.Messages;
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

        internal OutboundMessageEnvelope BuildEnvelope(IBasicProperties properties, Byte[] body)
        {
            var tag = _model.NextPublishSeqNo;
            return new OutboundMessageEnvelope(properties, tag, body);
        }

        internal Task<IPublishResult> PublishAsync(OutboundMessageEnvelope message,
                                                   Exchange exchange,
                                                   String routingKey = "",
                                                   TaskFactory taskFactory = null)
        {
            var factory = taskFactory ?? Task.Factory;

            return factory.StartNew(_ =>
                                    {
                                        _model.BasicPublish(exchange.Name,
                                                            routingKey,
                                                            false,
                                                            false,
                                                            (IBasicProperties)_,
                                                            message.Body);
                                    },
                                    message.Properties)
                          .ContinueWith(Result);
        }

        protected virtual void OnModelBasicReturn(Object sender, BasicReturnEventArgs args) { }

        protected virtual void OnModelBasicNacks(Object sender, BasicNackEventArgs args) { }

        protected virtual void OnModelBasicAcks(Object sender, BasicAckEventArgs args) { }

        private static IPublishResult Result(Task task)
        {
            if (task.Exception != null)
                return new FailurePublishing(task.Exception.GetBaseException());

            return SuccessfulPublishing.FromBasicProperties(task.AsyncState as IBasicProperties);
        }
    }
}