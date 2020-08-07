using System;
using System.Threading.Tasks;
using Carrot.Logging;
using Carrot.Messages;

namespace Carrot.Extensions
{
    internal static class ConsumerExtensions
    {
        internal static AggregateConsumingResult HandleErrorResult(this Task<AggregateConsumingResult> task,
                                                                   ILog log)
        {
            var result = task.Result;

            if (result is ConsumingFailureBase)
            {
                if (result is CorruptedMessageConsumingFailure)
                    log.Error("message content corruption detected");
                else if (result is UnresolvedMessageConsumingFailure)
                    log.Error("runtime type cannot be resolved");
                else if (result is UnsupportedMessageConsumingFailure)
                    log.Error("message type cannot be resolved");

                ((ConsumingFailureBase)result).WithErrors(_ => log.Error("consuming error",
                                                                         _.GetBaseException()));
            }

            return result;
        }

        internal static Task<ConsumedMessage.ConsumingResult> SafeConsumeAsync(this IConsumer consumer,
                                                                               ConsumedMessageBase message,
                                                                               IOutboundChannel outboundChannel)
        {
            try
            {
                var context = new ConsumingContext(message, outboundChannel);
                return consumer.ConsumeAsync(context)
                               .ContinueWith(_ =>
                                             {
                                                 if (_.Exception == null)
                                                     return new ConsumedMessage.Success(message, consumer);
                                             
                                                 return BuildFailure(consumer,
                                                                     message,
                                                                     _.Exception.GetBaseException());
                                             }, TaskContinuationOptions.RunContinuationsAsynchronously);
            }
            catch (Exception exception) { return Task.FromResult(BuildFailure(consumer, message, exception)); }
        }

        private static ConsumedMessage.ConsumingResult BuildFailure(IConsumer consumer,
                                                                    ConsumedMessageBase message,
                                                                    Exception exception)
        {
            consumer.OnError(exception);
            return new ConsumedMessage.Failure(message, consumer, exception);
        }
    }
}