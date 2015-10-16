namespace Carrot.Messages
{
    using System;
    using System.Threading.Tasks;

    using Carrot.Messaging;

    internal class ConsumerWrapper
    {
        private readonly IConsumer _consumer;

        internal ConsumerWrapper(IConsumer consumer)
        {
            this._consumer = consumer;
        }

        internal interface IConsumingResult
        {
        }

        internal Task<IConsumingResult> Consume(ConsumedMessageBase message)
        {
            return this._consumer.Consume(message)
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
                this.Exception = exception;
            }
        }

        internal class Success : IConsumingResult { }
    }
}