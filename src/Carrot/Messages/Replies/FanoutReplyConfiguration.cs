namespace Carrot.Messages.Replies
{
    public class FanoutReplyConfiguration : ReplyConfiguration
    {
        public FanoutReplyConfiguration(string excgangeName) : base("fanout", excgangeName, string.Empty)
        {
        }
    }
}