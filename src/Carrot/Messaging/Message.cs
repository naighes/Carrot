using System;
using System.Collections.Generic;

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

        public class HeaderCollection : Dictionary<String, Object>
        {
            public String MessageId
            {
                get { return this["id"] as String; }
            }
        }
    }
}