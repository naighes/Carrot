using Carrot.Configuration;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class QueueEntity
    {
        [Fact]
        public void Equality()
        {
            var q1 = new MessageQueue("queue", 
                                      new Mock<IModel>().Object, 
                                      new Mock<IMessageTypeResolver>().Object);
            var q2 = new MessageQueue("queue", 
                                      new Mock<IModel>().Object, 
                                      new Mock<IMessageTypeResolver>().Object);

            Assert.Equal(q1, q2);
            Assert.Equal(q1.GetHashCode(), q2.GetHashCode());

            var q3 = new MessageQueue("queue_3",
                                      new Mock<IModel>().Object,
                                      new Mock<IMessageTypeResolver>().Object);

            Assert.NotEqual(q1, q3);
            Assert.NotEqual(q1.GetHashCode(), q3.GetHashCode());
        }
    }
}