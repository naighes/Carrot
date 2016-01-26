namespace Carrot.Messages.Replies
{
    public class DirectReplyConfiguration : ReplyConfiguration
    {
        public DirectReplyConfiguration(string exchangeName, string routingKey) : base("direct", exchangeName, routingKey)
        {
        }
    }
}