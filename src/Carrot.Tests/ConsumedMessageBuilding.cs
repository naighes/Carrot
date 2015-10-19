using System;
using Carrot.Messages;
using Carrot.Messaging;
using Carrot.Serialization;
using Moq;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class ConsumedMessageBuilding
    {
        [Fact]
        public void CannotResolve()
        {
            const String type = "fake-type";
            var serializerFactory = new Mock<ISerializerFactory>();
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve(type)).Returns(EmptyMessageType.Instance);
            var builder = new ConsumedMessageBuilder(serializerFactory.Object, resolver.Object);
            var message = builder.Build(new BasicDeliverEventArgs
                                            {
                                                BasicProperties = new BasicProperties
                                                                      {
                                                                          Type = type
                                                                      }
                                            });
            Assert.IsType<UnresolvedMessage>(message);
        }

        [Fact]
        public void MissingContentType()
        {
            const String contentType = "application/null";
            var serializerFactory = new Mock<ISerializerFactory>();
            serializerFactory.Setup(_ => _.Create(contentType)).Returns(NullSerializer.Instance);
            var resolver = new Mock<IMessageTypeResolver>();
            var builder = new ConsumedMessageBuilder(serializerFactory.Object, resolver.Object);
            var message = builder.Build(new BasicDeliverEventArgs
                                            {
                                                BasicProperties = new BasicProperties
                                                                      {
                                                                          ContentType = contentType
                                                                      }
                                            });
            Assert.IsType<UnsupportedMessage>(message);
        }
    }
}