using System;

namespace Carrot.Messages.Replies
{
    public class TopicReplyConfiguration : ReplyConfiguration
    {
        public TopicReplyConfiguration(String exchangeName, String routingKey)
            : base("topic", exchangeName, routingKey)
        {
        }
    }
}