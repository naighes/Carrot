namespace Carrot.Messages.Replies
{
    public abstract class ReplyConfiguration
    {
        private readonly string _exchangeType;
        private readonly string _exchangeName;
        private readonly string _routingKey;

        protected ReplyConfiguration(string exchangeType, string exchangeName, string routingKey)
        {
            _exchangeType = exchangeType;
            _exchangeName = exchangeName;
            _routingKey = routingKey;
        }

        public override string ToString()
        {
            return $"{_exchangeType}://{_exchangeName}/{_routingKey}";
        }
    }
}