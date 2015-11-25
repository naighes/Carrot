using System;
using System.Threading.Tasks;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public interface IOutboundChannel : IDisposable
    {
        Task<IPublishResult> PublishAsync<TMessage>(IBasicProperties properties,
                                                    Byte[] body,
                                                    Exchange exchange,
                                                    String routingKey,
                                                    OutboundMessage<TMessage> source)
            where TMessage : class;
    }
}