namespace Carrot.Messages.Replies
{
    public class TopicReplyConfiguration : ReplyConfiguration
    {
        public TopicReplyConfiguration(string excgangeName, string routingKey) : base("topic", excgangeName, routingKey)
        {
        }
    }
}