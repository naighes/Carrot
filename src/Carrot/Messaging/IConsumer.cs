using System;
using System.Threading.Tasks;

namespace TowerBridge.Common.Infrastructure.Messaging
{
    public interface IConsumer
    {
        Task Consume(Object message);
    }
}