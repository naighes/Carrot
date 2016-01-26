namespace Carrot.Messages.Replies
{
    public class TopicReplyConfiguration : ReplyConfiguration
    {
        public TopicReplyConfiguration(string exchangeName, string routingKey) : base("topic", exchangeName, routingKey)
        {
        }
    }
}