using System;
using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Carrot.Messaging
{
    public class Message<TMessage> where TMessage : class
    {
        private readonly TMessage _content;
        private readonly HeaderCollection _headers;

        internal Message(TMessage content, HeaderCollection headers)
        {
            _content = content;
            _headers = headers;
        }

        public TMessage Content
        {
            get { return _content; }
        }

        public HeaderCollection Headers
        {
            get { return _headers; }
        }
    }

    public class HeaderCollection
    {
        private readonly IDictionary<String, Object> _dictionary;

        internal HeaderCollection()
            : this(new Dictionary<String, Object>())
        {
        }

        internal HeaderCollection(IDictionary<String, Object> dictionary)
        {
            _dictionary = dictionary;
        }

        public String MessageId
        {
            get { return _dictionary["message_id"] as String; }
        }

        public Int64 Timestamp
        {
            get { return (Int64)_dictionary["timestamp"]; }
        }

        public static HeaderCollection Parse(BasicDeliverEventArgs args)
        {
            return new HeaderCollection(new Dictionary<String, Object>
                                        {
                                            { "message_id", args.BasicProperties.MessageId },
                                            { "timestamp", args.BasicProperties.Timestamp.UnixTime }
                                        });
        }
    }
}