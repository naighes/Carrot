using Carrot.Messages;

namespace Carrot.Fallback
{
    public interface IFallbackStrategy
    {
        void Apply(IOutboundChannel channel, ConsumedMessageBase message);
    }
}