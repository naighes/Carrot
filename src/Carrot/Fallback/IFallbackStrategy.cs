using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Fallback
{
    public interface IFallbackStrategy
    {
        Task<IFallbackApplied> Apply(IOutboundChannel channel, ConsumedMessageBase message);
    }
}