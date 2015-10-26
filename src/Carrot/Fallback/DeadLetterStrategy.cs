using System;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot.Fallback
{
    public class DeadLetterStrategy : IFallbackStrategy
    {
        private readonly Func<String, String> _exchangeNameBuilder;

        public DeadLetterStrategy()
            : this(_ => String.Format("{0}::dle", _))
        {
        }

        public DeadLetterStrategy(Func<String, String> exchangeNameBuilder)
        {
            _exchangeNameBuilder = exchangeNameBuilder;
        }

        public void Apply(IModel model, ConsumedMessageBase message)
        {
            message.ForwardTo(model, _exchangeNameBuilder);
        }
    }
}