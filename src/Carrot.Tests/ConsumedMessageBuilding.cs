using System;
using System.Reflection;
using System.Text;
using Carrot.Configuration;
using Carrot.Messages;
using Carrot.Serialization;
using Moq;
using RabbitMQ.Client.Events;
using Xunit;

namespace Carrot.Tests
{
    public class ConsumedMessageBuilding
    {
        [Fact]
        public void CannotResolve()
        {
            const String type = "fake-type";
            var serializationConfiguration = new SerializationConfiguration();
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve(It.Is<ConsumedMessageContext>(__ => __.MessageType == type)))
                    .Returns(EmptyMessageBinding.Instance);
            var builder = new ConsumedMessageBuilder(serializationConfiguration, resolver.Object);
            var message = builder.Build(new BasicDeliverEventArgs
            {
                BasicProperties = BasicPropertiesStubber.Stub(_ => _.Type = type)
            });
            Assert.IsType<UnresolvedMessage>(message);
        }

        [Fact]
        public void ExceptionOnResolve()
        {
            const String type = "fake-type";
            var serializationConfiguration = new SerializationConfiguration();
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve(It.Is<ConsumedMessageContext>(__ => __.MessageType == type)))
                    .Throws<Exception>();
            var builder = new ConsumedMessageBuilder(serializationConfiguration, resolver.Object);
            var message = builder.Build(new BasicDeliverEventArgs
                                            {
                                                BasicProperties = BasicPropertiesStubber.Stub(_ => _.Type = type)
                                            });
            Assert.IsType<UnresolvedMessage>(message);
        }

        [Fact]
        public void MissingContentType()
        {
            const String contentType = "application/null";
            var serializationConfiguration = new SerializationConfigurationWrapper(NullSerializer.Instance);
            var resolver = new Mock<IMessageTypeResolver>();
            var builder = new ConsumedMessageBuilder(serializationConfiguration, resolver.Object);
            var message = builder.Build(new BasicDeliverEventArgs
                                            {
                                                BasicProperties = BasicPropertiesStubber.Stub(_ => _.ContentType = contentType)
                                            });
            Assert.IsType<UnsupportedMessage>(message);
        }

        [Fact]
        public void DeserializeThrows()
        {
            const String type = "fake-type";
            const String contentType = "application/null";
            var body = new Byte[] { };
            var args = new BasicDeliverEventArgs
                           {
                               Body = body,
                               BasicProperties = BasicPropertiesStubber.Stub(_ =>
                               {
                                   _.ContentType = contentType;
                                   _.Type = type;
                               })
                           };
            var context = ConsumedMessageContext.FromBasicDeliverEventArgs(args);
            var runtimeType = typeof(Foo).GetTypeInfo();
            var serializer = new Mock<ISerializer>();
            serializer.Setup(_ => _.Deserialize(body, runtimeType, new UTF8Encoding(true)))
                      .Throws(new Exception("boom"));
            var serializationConfiguration = new SerializationConfigurationWrapper(serializer.Object);
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve(context)).Returns(new MessageBinding(type, runtimeType));
            var builder = new ConsumedMessageBuilder(serializationConfiguration, resolver.Object);
            var message = builder.Build(args);
            Assert.IsType<CorruptedMessage>(message);
        }

        private class SerializationConfigurationWrapper : SerializationConfiguration
        {
            private readonly ISerializer _serializer;

            public SerializationConfigurationWrapper(ISerializer serializer)
            {
                _serializer = serializer;
            }

            internal override ISerializer Create(String contentType)
            {
                return _serializer;
            }
        }
    }
}