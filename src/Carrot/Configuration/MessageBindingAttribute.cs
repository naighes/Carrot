using System;

namespace Carrot.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageBindingAttribute : Attribute
    {
        private readonly String _messageType;
        private Int32 _expiresAfter = -1;

        public MessageBindingAttribute(String messageType)
        {
            _messageType = messageType;
        }

        public String MessageType
        {
            get { return _messageType; }
        }

        /// <summary>
        /// Gets or sets the number of seconds after which the message will expire. Use -1 to set infinite expiration.
        /// </summary>
        public Int32 ExpiresAfter
        {
            get { return _expiresAfter; }
            set { _expiresAfter = value; }
        }
    }
}