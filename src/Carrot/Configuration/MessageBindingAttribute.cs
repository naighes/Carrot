using System;

namespace Carrot.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageBindingAttribute : Attribute
    {
        public MessageBindingAttribute(String messageType)
        {
            MessageType = messageType;
        }

        public String MessageType { get; }

        /// <summary>
        /// Gets or sets the number of seconds after which the message will expire. Use -1 to set infinite expiration.
        /// </summary>
        public Int32 ExpiresAfter { get; set; } = -1;
    }
}