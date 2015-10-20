using System;
using Carrot.Messages;
using Carrot.Serialization;
using Moq;
using RabbitMQ.Client;
using Xunit;

namespace Carrot.Tests
{
    public class OutboundEnveloping
    {
        [Fact]
        public void PublishingSuccessfully()
        {
            var content = new Foo();
            var message = new OutboundMessage<Foo>(content);
            var serializerFactory = new Mock<ISerializerFactory>();
            var serializer = new Mock<ISerializer>();
            serializer.Setup(_ => _.Serialize(content)).Returns("{}");
            serializerFactory.Setup(_ => _.Create("application/json")).Returns(serializer.Object);
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            var newId = new Mock<INewId>();
            const String messageId = "one-id";
            newId.Setup(_ => _.Next()).Returns(messageId);
            var model = new Mock<IModel>();
            var wrapper = new OutboundMessageEnvelope<Foo>(message,
                                                           serializerFactory.Object,
                                                           dateTimeProvider.Object,
                                                           newId.Object);
            var result = Assert.IsType<SuccessfulPublishing>(wrapper.PublishAsync(model.Object, "target_exchange").Result);
            Assert.Equal(messageId, result.MessageId);
        }

        [Fact]
        public void PublishingFailed()
        {
            const String exchange = "target_exchange";
            var content = new Foo();
            var message = new OutboundMessage<Foo>(content);
            var serializerFactory = new Mock<ISerializerFactory>();
            var serializer = new Mock<ISerializer>();
            serializer.Setup(_ => _.Serialize(content)).Returns("{}");
            serializerFactory.Setup(_ => _.Create("application/json")).Returns(serializer.Object);
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            var newId = new Mock<INewId>();
            var model = new Mock<IModel>();
            var exception = new Exception();
            model.Setup(_ => _.BasicPublish(exchange, 
                                            String.Empty, 
                                            It.IsAny<IBasicProperties>(), 
                                            It.IsAny<Byte[]>()))
                 .Throws(exception);
            var wrapper = new OutboundMessageEnvelope<Foo>(message, 
                                                           serializerFactory.Object,
                                                           dateTimeProvider.Object,
                                                           newId.Object);
            var result = Assert.IsType<FailurePublishing>(wrapper.PublishAsync(model.Object, exchange).Result);
            Assert.Equal(result.Exception, exception);
        }

        internal class OutboundMessageEnvelopeWrapper<TMessage> : OutboundMessageEnvelope<TMessage>
        {
            internal OutboundMessageEnvelopeWrapper(OutboundMessage<TMessage> message, 
                                                    ISerializerFactory serializerFactory, 
                                                    IDateTimeProvider dateTimeProvider, 
                                                    INewId newId)
                : base(message, serializerFactory, dateTimeProvider, newId)
            {
            }
        }
    }
}