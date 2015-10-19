using System.Threading.Tasks;
using Carrot.Extensions;
using Carrot.Messages;

namespace Carrot.Messaging
{
    internal class OuterConsumer
    {
        private readonly IConsumer _innerConsumer;

        public OuterConsumer(IConsumer innerConsumer)
        {
            _innerConsumer = innerConsumer;
        }

        public Task<ConsumedMessage.ConsumingResult> ConsumeAsync(ConsumedMessage message)
        {
            return Task<Task>.Factory
                             .StartNew(_ => _innerConsumer.ConsumeAsync(_), message)
                             .Unwrap()
                             .ContinueWith(_ => ConsumingResult(_, message));
        }

        private ConsumedMessage.ConsumingResult ConsumingResult(Task task, ConsumedMessage message)
        {
            if (task.Exception == null)
                return new ConsumedMessage.Success(message);

            var exception = task.Exception.GetBaseException();
            _innerConsumer.OnError(exception);
            return new ConsumedMessage.Failure(message, exception);
        }
    }
}