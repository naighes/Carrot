using RabbitMQ.Client;

namespace Carrot.Tests
{
    public static class BasicPropertiesStubber
    {
        public static IBasicProperties Stub()
        {
            using (var c = new ConnectionFactory().CreateConnection())
            using (IModel m = c.CreateModel())
            {
                return m.CreateBasicProperties();
            }
        }
    }
}