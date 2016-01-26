namespace Carrot.Messages.Replies
{
    public abstract class ReplyConfiguration
    {
        protected ReplyConfiguration(string exchangeType, string excgangeName, string routingKey)
        {
            RoutingKey = routingKey;
            ExcgangeName = excgangeName;
            ExchangeType = exchangeType;
        }

        public string ExchangeType { get; }
        public string ExcgangeName { get; }

        public string RoutingKey { get; }
    }
}