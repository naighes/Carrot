namespace Carrot.Messages.Replies
{
    public abstract class ReplyConfiguration
    {
        protected ReplyConfiguration(string exchangeType, string exchangeName, string routingKey)
        {
            ExchangeType = exchangeType;
            ExchangeName = exchangeName;
            RoutingKey = routingKey;
        }

        public string ExchangeType { get; }

        public string ExchangeName { get; }

        public string RoutingKey { get; }

        public override string ToString()
        {
            return $"{ExchangeType}://{ExchangeName}/{RoutingKey}";
        }
    }
}