using System;
using System.Threading.Tasks;
using Carrot.Messaging;

namespace Carrot.Messages
{
    internal class ConsumerWrapper
    {
        private readonly IConsumer _consumer;

        internal ConsumerWrapper(IConsumer consumer)
        {
            _consumer = consumer;
        }

        internal interface IConsumingResult
        {
        }

        internal Task<IConsumingResult> Consume(ConsumedMessageBase message)
        {
            return _consumer.Consume(message)
                            .ContinueWith<IConsumingResult>(_ =>
                                                            {
                                                                if (_.Exception != null)
                                                                    return new Failure(_.Exception.GetBaseException());

                                                                return new Success();
                                                            });
        }

        internal class Failure : IConsumingResult
        {
            internal readonly Exception Exception;

            internal Failure(Exception exception)
            {
                Exception = exception;
            }
        }

        internal class Success : IConsumingResult { }
    }
}