using Carrot.Messages;

namespace Carrot.Fallback
{
    public interface IFallbackStrategy
    {
        void Apply(IOutboundChannelPool channelPool, ConsumedMessageBase message);
    }
}