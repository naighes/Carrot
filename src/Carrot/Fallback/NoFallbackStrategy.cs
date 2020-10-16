using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Fallback
{
    internal class NoFallbackStrategy : IFallbackStrategy
    {
        internal static readonly IFallbackStrategy Instance = new NoFallbackStrategy();

        private NoFallbackStrategy() { }

        public Task<IFallbackApplied> Apply(IOutboundChannel channel, ConsumedMessageBase message)
        {
            return Task.FromResult<IFallbackApplied>(new FallbackAppliedSuccessful());
        }
    }
}