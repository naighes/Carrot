using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Messaging
{
    public interface IConsumer
    {
        Task Consume(ConsumedMessageBase message);
    }
}