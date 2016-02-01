using System;

namespace Carrot.Messages.Replies
{
    public class DirectReplyConfiguration : ReplyConfiguration
    {
        public DirectReplyConfiguration(String routingKey)
            : this(String.Empty, routingKey)
        {
        }

        public DirectReplyConfiguration(String exchangeName, String routingKey)
            : base("direct", exchangeName, routingKey)
        {
        }
    }
}