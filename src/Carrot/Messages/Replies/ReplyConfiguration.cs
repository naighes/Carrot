using System;

namespace Carrot.Messages.Replies
{
    public abstract class ReplyConfiguration
    {
        protected ReplyConfiguration(String exchangeType, String exchangeName, String routingKey)
        {
            ExchangeType = exchangeType;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }

        public String ExchangeType { get; }

        public String ExchangeName { get; }

        public String RoutingKey { get; }

        public override String ToString()
        {
            return $"{ExchangeType}://{ExchangeName}/{RoutingKey}";
        }
    }
}