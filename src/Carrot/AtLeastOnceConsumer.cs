using System;
using System.Threading.Tasks;
using Carrot.Configuration;
using Carrot.Messages;
using RabbitMQ.Client.Events;

namespace Carrot
{
    public class AtLeastOnceConsumer : ConsumerBase
    {
        internal AtLeastOnceConsumer(IInboundChannel inboundChannel,
                                     IOutboundChannel outboundChannel,
                                     Queue queue,
                                     IConsumedMessageBuilder builder,
                                     ConsumingConfiguration configuration)
            : base(inboundChannel, outboundChannel, queue, builder, configuration)
        {
        }

        protected override Task<AggregateConsumingResult> ConsumeInternalAsync(BasicDeliverEventArgs args)
        {
            return ConsumeAsync(args,
                                OutboundChannel).ContinueWith(_ =>
                                                              {
                                                                  var result = _.Result;
                                                              
                                                                  try
                                                                  {
                                                                      var consumingResult = result.Reply(InboundChannel, OutboundChannel);

                                                                      if (consumingResult is ReiteratedConsumingFailure)
                                                                          return Fallback(consumingResult);

                                                                      result.NotifyConsumingCompletion();
                                                                  }
                                                                  catch (Exception e)
                                                                  {
                                                                      result.NotifyConsumingFault(e);
                                                                  }
                                                                  return Task.FromResult(result);
                                                              }).Unwrap();
        }

        Task<AggregateConsumingResult> Fallback(AggregateConsumingResult failureResult)
        {
            return Configuration.FallbackStrategy
                                .Apply(OutboundChannel, 
                                       failureResult.Message)
                                .ContinueWith(t =>
                                              {
                                                  if (t.Result.Success)
                                                      failureResult.Message.Acknowledge(InboundChannel);
                                                  else
                                                      failureResult.Message.Requeue(InboundChannel);

                                                  return failureResult;
                                              },
                                              TaskContinuationOptions.RunContinuationsAsynchronously)
                                .ContinueWith(t =>
                                              {
                                                  var exception = t.Exception?.GetBaseException();

                                                  if (exception != null)
                                                      failureResult.NotifyConsumingFault(exception);
                                                  else
                                                      failureResult.NotifyConsumingCompletion();

                                                  return failureResult;
                                              },
                                              TaskContinuationOptions.RunContinuationsAsynchronously);
        }
    }
}