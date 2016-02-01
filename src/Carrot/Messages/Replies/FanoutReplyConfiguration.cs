using System;

namespace Carrot.Messages.Replies
{
    public class FanoutReplyConfiguration : ReplyConfiguration
    {
        public FanoutReplyConfiguration(String exchangeName)
            : base("fanout", exchangeName, String.Empty)
        {
        }
    }
}