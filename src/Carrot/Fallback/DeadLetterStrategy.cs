using RabbitMQ.Client;

namespace Carrot.Fallback
{
    public class DeadLetterStrategy : IFallbackStrategy
    {
        public void Apply(IModel model)
        {
        }
    }
}