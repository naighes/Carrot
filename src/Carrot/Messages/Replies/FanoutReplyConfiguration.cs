namespace Carrot.Messages.Replies
{
    public class FanoutReplyConfiguration : ReplyConfiguration
    {
        public FanoutReplyConfiguration(string excgangeName, string routingKey) : base("fanout", excgangeName, routingKey)
        {
        }
    }
}