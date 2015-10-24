using RabbitMQ.Client;

namespace Carrot.Fallback
{
    internal class NoFallbackStrategy : IFallbackStrategy
    {
        internal static IFallbackStrategy Instance = new NoFallbackStrategy();

        private NoFallbackStrategy() { }

        public void Apply(IModel model) { }
    }
}