using Carrot.Messages;
using Moq;
using Xunit;

namespace Carrot.Tests
{
    public class QueueEntity
    {
        [Fact]
        public void Equality()
        {
            var q1 = MessageQueue.New("queue", new Mock<IConsumedMessageBuilder>().Object);
            var q2 = MessageQueue.New("queue", new Mock<IConsumedMessageBuilder>().Object);

            Assert.Equal(q1, q2);
            Assert.Equal(q1.GetHashCode(), q2.GetHashCode());

            var q3 = MessageQueue.New("queue_3", new Mock<IConsumedMessageBuilder>().Object);

            Assert.NotEqual(q1, q3);
            Assert.NotEqual(q1.GetHashCode(), q3.GetHashCode());
        }
    }
}