using System;

namespace Carrot
{
    public interface IOutboundChannelPool : IDisposable
    {
        IOutboundChannel Take();
        void Add(IOutboundChannel channel);
    }
}