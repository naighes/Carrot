using System;
using System.Threading.Tasks;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public interface IOutboundChannel : IDisposable
    {
        Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source,
                                                    IBasicProperties properties,
                                                    Byte[] body,
                                                    Exchange exchange,
                                                    String routingKey)
            where TMessage : class;
    }
}