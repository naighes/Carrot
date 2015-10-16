using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carrot.Messages;

namespace Carrot.Messaging
{
    public abstract class Consumer<TMessage> : IConsumer where TMessage : class
    {
        private readonly IDictionary<Type, Func<Exception>> _errorMap = new Dictionary<Type, Func<Exception>>
                                                                            {
                                                                                { typeof(UnresolvedMessage), () => new Exception("message cannot be resolved") }
                                                                            };

        public abstract Task Consume(Message<TMessage> message);

        Task IConsumer.Consume(ConsumedMessageBase message)
        {
            if (!(message is ConsumedMessage)) 
                throw _errorMap[message.GetType()]();

            return Consume(message.As<TMessage>());
        }
    }

    public class Message<TMessage> where TMessage : class
    {
        private readonly TMessage _content;
        private readonly HeaderCollection _headers;

        internal Message(TMessage content, HeaderCollection headers)
        {
            _content = content;
            _headers = headers;
        }

        public class HeaderCollection : Dictionary<String, Object>
        {
            public String MessageId
            {
                get { return this["id"] as String; }
            }
        }
    }
}