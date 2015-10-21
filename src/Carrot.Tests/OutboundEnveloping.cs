using System;
using Carrot.Extensions;
using Carrot.Messages;
using Carrot.Serialization;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
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

        [Fact]
        public void BasicPropertiesMapping()
        {
            const String messageId = "one-id";
            var timestamp = new DateTimeOffset(2015, 1, 2, 3, 4, 5, TimeSpan.Zero);
            var properties = new BasicProperties();
            var newId = new Mock<INewId>();
            newId.Setup(_ => _.Next()).Returns(messageId);
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(_ => _.UtcNow()).Returns(timestamp);
            var envelope = new OutboundMessageEnvelopeWrapper<Foo>(new OutboundMessage<Foo>(new Foo()), 
                                                                   new Mock<ISerializerFactory>().Object, 
                                                                   dateTimeProvider.Object, 
                                                                   newId.Object);
            envelope.CallHydrateProperties(properties);
            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(timestamp.ToUnixTimestamp(), properties.Timestamp.UnixTime);
        }

        [Fact]
        public void MessageType()
        {
            var envelope = new OutboundMessageEnvelopeWrapper<Foo>(new OutboundMessage<Foo>(new Foo()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("urn:message:foo", properties.Type);
        }

        [Fact]
        public void MessageTypeFallback()
        {
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new OutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("urn:message:Carrot.Tests.Bar", properties.Type);
        }

        [Fact]
        public void ContentEncoding()
        {
            const String contentEncoding = "UTF-16";
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new OutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties { ContentEncoding = contentEncoding };
            envelope.CallHydrateProperties(properties);
            Assert.Equal(contentEncoding, properties.ContentEncoding);
        }

        [Fact]
        public void DefaultContentEncoding()
        {
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new OutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("UTF-8", properties.ContentEncoding);
        }

        [Fact]
        public void ContentType()
        {
            const String contentType = "application/xml";
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new OutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties { ContentType = contentType };
            envelope.CallHydrateProperties(properties);
            Assert.Equal(contentType, properties.ContentType);
        }

        [Fact]
        public void DefaultContentType()
        {
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new OutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("application/json", properties.ContentType);
        }

        [Fact]
        public void NonDurableMessage()
        {
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new OutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.False(properties.Persistent);
        }

        [Fact]
        public void DurableMessage()
        {
            var envelope = new OutboundMessageEnvelopeWrapper<Bar>(new DurableOutboundMessage<Bar>(new Bar()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   new Mock<IDateTimeProvider>().Object,
                                                                   new Mock<INewId>().Object);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.True(properties.Persistent);
        }

        internal class OutboundMessageEnvelopeWrapper<TMessage> : OutboundMessageEnvelope<TMessage> where TMessage : class
        {
            internal OutboundMessageEnvelopeWrapper(OutboundMessage<TMessage> message, 
                                                    ISerializerFactory serializerFactory, 
                                                    IDateTimeProvider dateTimeProvider, 
                                                    INewId newId)
                : base(message, serializerFactory, dateTimeProvider, newId)
            {
            }

            internal void CallHydrateProperties(IBasicProperties properties)
            {
                HydrateProperties(properties);
            }
        }
    }
}