using System;

namespace Carrot.Messages.Replies
{
    internal static class ReplyConfigurationFactory
    {
        public static ReplyConfiguration Create(String exchangeType, String exchangeName, String routingKey)
        {
            switch (exchangeType.ToLowerInvariant())
            {
                case "direct":
                    return new DirectReplyConfiguration(exchangeName, routingKey);
                case "topic":
                    return new TopicReplyConfiguration(exchangeName, routingKey);
                case "fanout":
                    return new FanoutReplyConfiguration(exchangeName);
            }

            throw new ArgumentException($"Exchange type not recognized: {exchangeType}");
        }
    }
}