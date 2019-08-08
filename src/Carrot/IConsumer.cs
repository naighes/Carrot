using System;
using System.Threading.Tasks;

namespace Carrot
{
    public interface IConsumer
    {
        Task ConsumeAsync(ConsumingContext context);

        void OnError(Exception exception);

        void OnConsumeCompletion();
    }
}