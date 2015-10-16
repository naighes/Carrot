namespace Carrot.Messaging
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageBindingAttribute : Attribute
    {
        private readonly String _messageType;

        public MessageBindingAttribute(String messageType)
        {
            this._messageType = messageType;
        }

        public String MessageType
        {
            get { return this._messageType; }
        }
    }
}