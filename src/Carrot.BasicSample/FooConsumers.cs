using System;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.BasicSample
{
    internal class PublishingFooConsumer : Consumer<Foo>
    {
        private readonly Exchange _exchange;
        private readonly String _routingKey;

        public PublishingFooConsumer(Exchange exchange, String routingKey)
        {
            _exchange = exchange;
            _routingKey = routingKey;
        }

        public override Task ConsumeAsync(ConsumingContext<Foo> context)
        {
            return Task.Factory
                       .StartNew(() =>
                                 {
                                     Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                                       context.Message.ConsumerTag,
                                                       context.Message.Headers.MessageId,
                                                       GetType().Name);
                                     return context.OutboundChannel
                                                   .PublishAsync<Foo>(new OutboundMessage<Foo>(context.Message
                                                                                                      .Content),
                                                                      _exchange,
                                                                      _routingKey);
                                 })
                       .Unwrap()
                       .ContinueWith(_ =>
                                     {
                                         var result = _.Result;

                                         if (result is SuccessfulPublishing)
                                             Console.WriteLine("published message '{0}'",
                                                               ((SuccessfulPublishing)result).MessageId);
                                         else
                                         {
                                             Console.WriteLine("an error has occurred while publishing message: {0}",
                                                               ((FailurePublishing)result).Exception.Message);
                                             throw ((FailurePublishing)result).Exception;
                                         }
                                     });
        }
    }

    internal class FooConsumer : Consumer<Foo>
    {
        public override Task ConsumeAsync(ConsumingContext<Foo> context)
        {
            return Task.Factory
                       .StartNew(() =>
                                 {
                                     Console.WriteLine("[{0}]received '{1}' by '{2}'",
                                                       context.Message.ConsumerTag,
                                                       context.Message.Headers.MessageId,
                                                       GetType().Name);
                                 });
        }
    }
}