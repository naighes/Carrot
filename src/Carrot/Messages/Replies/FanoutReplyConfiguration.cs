namespace Carrot.Messages.Replies
{
    public class FanoutReplyConfiguration : ReplyConfiguration
    {
        public FanoutReplyConfiguration(string exchangeName) : base("fanout", exchangeName, string.Empty)
        {
        }
    }
}