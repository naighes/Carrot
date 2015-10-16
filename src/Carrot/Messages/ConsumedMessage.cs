using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TowerBridge.Common.Infrastructure.Messaging;

namespace TowerBridge.Common.Infrastructure.Messages
{
    public class ConsumedMessage : ConsumedMessageBase
    {
        private readonly Object _content;

        internal ConsumedMessage(Object content, 
                                 String messageId, 
                                 UInt64 deliveryTag, 
                                 Boolean redelivered)
            : base(messageId, deliveryTag, redelivered)
        {
            _content = content;
        }

        public Object Content
        {
            get { return _content; }
        }

        private Boolean Match(Type type)
        {
            return Content != null && type.IsInstanceOfType(Content);
        }

        internal override Task<IAggregateConsumingResult> Consume(IDictionary<Type, IConsumer> subscriptions)
        {
            return Task.WhenAll(subscriptions.Where(_ => Match(_.Key))
                                             .Select(_ => new ConsumerWrapper(_.Value).Consume(this)))
                       .ContinueWith(_ => _.Result
                                           .OfType<ConsumerWrapper.Failure>()
                                           .Any() ? Failure.Build(_.Result) : (IAggregateConsumingResult)new Success());
        }
    }

    public interface IAggregateConsumingResult
    {
    }

    public class Failure : IAggregateConsumingResult
    {
        private readonly Exception[] _exceptions;

        protected Failure(params Exception[] exceptions)
        {
            _exceptions = exceptions;
        }

        public Exception[] Exceptions
        {
            get { return _exceptions ?? new Exception[] { }; }
        }

        internal static IAggregateConsumingResult Build(ConsumerWrapper.IConsumingResult[] results)
        {
            return new Failure(results.OfType<ConsumerWrapper.Failure>()
                                      .Select(_ => _.Exception)
                                      .ToArray());
        }
    }

    public class Success : IAggregateConsumingResult
    {
    }
}