using System;
using Carrot.Configuration;
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
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(new MessageBinding("urn:message:fake", typeof(Foo)));
            var wrapper = new OutboundMessageEnvelope<Foo>(message,
                                                           serializerFactory.Object,
                                                           dateTimeProvider.Object,
                                                           newId.Object,
                                                           resolver.Object);
            var result = Assert.IsType<SuccessfulPublishing>(wrapper.PublishAsync(model.Object,
                                                                                  Exchange.Direct("target_exchange")).Result);
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
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(new MessageBinding("urn:message:fake", typeof(Foo)));
            var wrapper = new OutboundMessageEnvelope<Foo>(message,
                                                           serializerFactory.Object,
                                                           dateTimeProvider.Object,
                                                           newId.Object,
                                                           resolver.Object);
            var result = Assert.IsType<FailurePublishing>(wrapper.PublishAsync(model.Object,
                                                                               Exchange.Direct(exchange)).Result);
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
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(new MessageBinding("urn:message:fake", typeof(Foo)));
            var envelope = new OutboundMessageEnvelopeWrapper<Foo>(new OutboundMessage<Foo>(new Foo()),
                                                                   new Mock<ISerializerFactory>().Object,
                                                                   dateTimeProvider.Object,
                                                                   newId.Object,
                                                                   resolver.Object);
            envelope.CallHydrateProperties(properties);
            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(timestamp.ToUnixTimestamp(), properties.Timestamp.UnixTime);
        }

        [Fact]
        public void MessageType()
        {
            var envelope = BuildDefaultEnvelope(new OutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("urn:message:fake", properties.Type);
        }

        [Fact]
        public void ContentEncoding()
        {
            const String contentEncoding = "UTF-16";
            var envelope = BuildDefaultEnvelope(new OutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties { ContentEncoding = contentEncoding };
            envelope.CallHydrateProperties(properties);
            Assert.Equal(contentEncoding, properties.ContentEncoding);
        }

        [Fact]
        public void DefaultContentEncoding()
        {
            var envelope = BuildDefaultEnvelope(new OutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("UTF-8", properties.ContentEncoding);
        }

        [Fact]
        public void ContentType()
        {
            const String contentType = "application/xml";
            var envelope = BuildDefaultEnvelope(new OutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties { ContentType = contentType };
            envelope.CallHydrateProperties(properties);
            Assert.Equal(contentType, properties.ContentType);
        }

        [Fact]
        public void DefaultContentType()
        {
            var envelope = BuildDefaultEnvelope(new OutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("application/json", properties.ContentType);
        }

        [Fact]
        public void NonDurableMessage()
        {
            var envelope = BuildDefaultEnvelope(new OutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.False(properties.Persistent);
        }

        [Fact]
        public void DurableMessage()
        {
            var envelope = BuildDefaultEnvelope(new DurableOutboundMessage<Bar>(new Bar()));
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.True(properties.Persistent);
        }

        [Fact]
        public void MessageExpiration()
        {
            var expiresAfter = new TimeSpan?(TimeSpan.FromSeconds(18));
            var envelope = BuildDefaultEnvelope(new DurableOutboundMessage<Bar>(new Bar()), expiresAfter);
            var properties = new BasicProperties();
            envelope.CallHydrateProperties(properties);
            Assert.Equal("18000", properties.Expiration);
        }

        private static OutboundMessageEnvelopeWrapper<TMessage> BuildDefaultEnvelope<TMessage>(OutboundMessage<TMessage> message,
                                                                                               TimeSpan? expiresAfter = null)
            where TMessage : class
        {
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<TMessage>())
                    .Returns(new MessageBinding("urn:message:fake", typeof(TMessage), expiresAfter));
            return new OutboundMessageEnvelopeWrapper<TMessage>(message,
                                                                new Mock<ISerializerFactory>().Object,
                                                                new Mock<IDateTimeProvider>().Object,
                                                                new Mock<INewId>().Object,
                                                                resolver.Object);
        }

        internal class OutboundMessageEnvelopeWrapper<TMessage> : OutboundMessageEnvelope<TMessage> where TMessage : class
        {
            internal OutboundMessageEnvelopeWrapper(OutboundMessage<TMessage> message,
                                                    ISerializerFactory serializerFactory,
                                                    IDateTimeProvider dateTimeProvider,
                                                    INewId newId,
                                                    IMessageTypeResolver resolver)
                : base(message, serializerFactory, dateTimeProvider, newId, resolver)
            {
            }

            internal void CallHydrateProperties(IBasicProperties properties)
            {
                HydrateProperties(properties);
            }
        }
    }
}