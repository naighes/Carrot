using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot
{
    public interface IConsumer
    {
        Task ConsumeAsync(ConsumedMessageBase message);

        void OnError(Exception exception);

        void OnConsumeCompletion();
    }
}