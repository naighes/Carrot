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

            return Consume(Message<TMessage>.Build(message));
        }
    }

    public class Message<TMessage> where TMessage : class
    {
        private readonly TMessage _content;

        private Message(TMessage content)
        {
            _content = content;
        }

        public static Message<TMessage> Build(ConsumedMessageBase message)
        {
            var content = message.Content as TMessage;

            // TODO: check is proper type.

            return new Message<TMessage>(content);
        }
    }
}