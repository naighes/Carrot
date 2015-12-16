using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot
{
    public interface IOutboundChannel : IDisposable
    {
        Task<IPublishResult> PublishAsync<TMessage>(OutboundMessage<TMessage> source,
                                                    Exchange exchange,
                                                    String routingKey)
            where TMessage : class;

        Task<IPublishResult> ForwardAsync(ConsumedMessageBase message,
                                          Exchange exchange,
                                          String routingKey);
    }
}