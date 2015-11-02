using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Extensions
{
    internal static class ConsumerExtensions
    {
        internal static Task<ConsumedMessage.ConsumingResult> SafeConsumeAsync(this IConsumer consumer,
                                                                               ConsumedMessage message)
        {
            try
            {
                return consumer.ConsumeAsync(message)
                               .ContinueWith(_ =>
                                             {
                                                 if (_.Exception == null)
                                                     return new ConsumedMessage.Success(message);
                                             
                                                 return BuildFailure(consumer,
                                                                     message,
                                                                     _.Exception.GetBaseException());
                                             });
            }
            catch (Exception exception) { return Task.FromResult(BuildFailure(consumer, message, exception)); }
        }

        private static ConsumedMessage.ConsumingResult BuildFailure(IConsumer consumer,
                                                                    ConsumedMessage message,
                                                                    Exception exception)
        {
            consumer.OnError(exception);
            return new ConsumedMessage.Failure(message, exception);
        }
    }
}