using System;

namespace Carrot.Messaging
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageBindingAttribute : Attribute
    {
        private readonly String _messageType;

        public MessageBindingAttribute(String messageType)
        {
            _messageType = messageType;
        }

        public String MessageType
        {
            get { return _messageType; }
        }
    }
}