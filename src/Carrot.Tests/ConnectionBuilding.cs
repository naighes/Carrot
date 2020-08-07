using System;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class ConnectionBuilding
    {
        [Fact]
        public void Factory()
        {
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            var date = new DateTimeOffset(2015, 11, 12, 3, 4, 5, 6, TimeSpan.Zero);
            dateTimeProvider.Setup(_ => _.UtcNow()).Returns(date);
            var builder = new ConnectionBuilderWrapper(dateTimeProvider.Object);
            var factory = builder.CallBuildConnectionFactory(new Uri("amqp://localhost", UriKind.Absolute));
            Assert.Equal("Thu, 12 Nov 2015 03:04:05 GMT", factory.ClientProperties["connected_on"].ToString());
            Assert.Equal("Carrot", factory.ClientProperties["client_api"].ToString());
        }

        private class ConnectionBuilderWrapper : ConnectionBuilder
        {
            public ConnectionBuilderWrapper(IDateTimeProvider dateTimeProvider)
                : base(dateTimeProvider)
            {
            }

            internal ConnectionFactory CallBuildConnectionFactory(Uri endpointUri)
            {
                return BuildConnectionFactory(endpointUri);
            }
        }
    }
}