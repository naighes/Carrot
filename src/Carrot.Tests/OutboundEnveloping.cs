using System;
using Carrot.Configuration;
using Carrot.Extensions;
using Carrot.Messages;
using Carrot.Serialization;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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
            var dateTimeProvider = new Mock<IDateTimeProvider>();

            var serializer = new Mock<ISerializer>();
            serializer.Setup(_ => _.Serialize(content)).Returns("{}");

            const String messageId = "one-id";
            var newId = new Mock<INewId>();
            newId.Setup(_ => _.Next()).Returns(messageId);

            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(new MessageBinding("urn:message:fake", typeof(Foo)));

            var model = new Mock<IModel>();

            var configuration = new ChannelConfiguration();
            configuration.GeneratesMessageIdBy(newId.Object);
            configuration.ResolveMessageTypeBy(resolver.Object);
            configuration.ConfigureSerialization(_ =>
                                                 {
                                                     _.Map(__ => __.MediaType == "application/json", serializer.Object);
                                                 });
            var properties = message.BuildBasicProperties(resolver.Object, dateTimeProvider.Object, newId.Object);
            const UInt64 deliveryTag = 0uL;
            var envelope = new OutboundMessageEnvelope<Foo>(properties,
                                                            new Byte[] { },
                                                            new Exchange("target_exchange", "direct"),
                                                            null,
                                                            deliveryTag,
                                                            message);
            var channel = new OutboundChannelWrapper(model.Object);
            var task = channel.PublishAsync(envelope);
            channel.CallOnModelBasicAcks(new BasicAckEventArgs { DeliveryTag = deliveryTag });
            var result = Assert.IsType<SuccessfulPublishing>(task.Result);
            Assert.Equal(messageId, result.MessageId);
        }

        [Fact]
        public void PublishingFailed()
        {
            const String exchange = "target_exchange";
            var content = new Foo();
            var message = new OutboundMessage<Foo>(content);
            var dateTimeProvider = new Mock<IDateTimeProvider>();

            var serializer = new Mock<ISerializer>();
            serializer.Setup(_ => _.Serialize(content)).Returns("{}");

            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(new MessageBinding("urn:message:fake", typeof(Foo)));

            var model = new Mock<IModel>();

            var exception = new Exception();
            model.Setup(_ => _.BasicPublish(exchange,
                                            String.Empty,
                                            false,
                                            false,
                                            It.IsAny<IBasicProperties>(),
                                            It.IsAny<Byte[]>()))
                 .Throws(exception);

            var configuration = new ChannelConfiguration();
            configuration.GeneratesMessageIdBy(new Mock<INewId>().Object);
            configuration.ResolveMessageTypeBy(resolver.Object);
            configuration.ConfigureSerialization(_ =>
                                                 {
                                                     _.Map(__ => __.MediaType == "application/json",
                                                           serializer.Object);
                                                 });

            var properties = message.BuildBasicProperties(resolver.Object,
                                                          dateTimeProvider.Object,
                                                          new Mock<INewId>().Object);
            var envelope = new OutboundMessageEnvelope<Foo>(properties,
                                                            new Byte[] { },
                                                            new Exchange(exchange, "direct"),
                                                            String.Empty,
                                                            0uL,
                                                            message);
            var channel = new OutboundChannel(model.Object);
            var result = Assert.IsType<FailurePublishing>(channel.PublishAsync(envelope).Result);
            Assert.Equal(result.Exception, exception);
        }

        [Fact]
        public void BasicPropertiesMapping()
        {
            const String messageId = "one-id";
            var timestamp = new DateTimeOffset(2015, 1, 2, 3, 4, 5, TimeSpan.Zero);
            
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(_ => _.UtcNow()).Returns(timestamp);

            var newId = new Mock<INewId>();
            newId.Setup(_ => _.Next()).Returns(messageId);

            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<Foo>()).Returns(new MessageBinding("urn:message:fake", typeof(Foo)));

            var configuration = new ChannelConfiguration();
            configuration.GeneratesMessageIdBy(newId.Object);
            configuration.ResolveMessageTypeBy(resolver.Object);

            var message = new OutboundMessage<Foo>(new Foo());
            var properties = message.BuildBasicProperties(resolver.Object,
                                                          dateTimeProvider.Object,
                                                          newId.Object);
            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(timestamp.ToUnixTimestamp(), properties.Timestamp.UnixTime);
        }

        [Fact]
        public void MessageType()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.Equal("urn:message:fake", properties.Type);
        }

        [Fact]
        public void ContentEncoding()
        {
            const String contentEncoding = "UTF-16";
            var message = new OutboundMessage<Bar>(new Bar());
            message.Headers.SetContentEncoding(contentEncoding);
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.Equal(contentEncoding, properties.ContentEncoding);
        }

        [Fact]
        public void DefaultContentEncoding()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.Equal("UTF-8", properties.ContentEncoding);
        }

        [Fact]
        public void ContentType()
        {
            const String contentType = "application/xml";
            var message = new OutboundMessage<Bar>(new Bar());
            message.Headers.SetContentType(contentType);
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.Equal(contentType, properties.ContentType);
        }

        [Fact]
        public void DefaultContentType()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.Equal("application/json", properties.ContentType);
        }

        [Fact]
        public void NonDurableMessage()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.False(properties.Persistent);
        }

        [Fact]
        public void DurableMessage()
        {
            var message = new DurableOutboundMessage<Bar>(new Bar());
            var properties = message.BuildBasicProperties(StubResolver<Bar>(null).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.True(properties.Persistent);
        }

        [Fact]
        public void MessageExpiration()
        {
            var expiresAfter = new TimeSpan?(TimeSpan.FromSeconds(18));
            var message = new DurableOutboundMessage<Bar>(new Bar());
            var properties = message.BuildBasicProperties(StubResolver<Bar>(expiresAfter).Object,
                                                          new Mock<IDateTimeProvider>().Object,
                                                          new Mock<INewId>().Object);
            Assert.Equal("18000", properties.Expiration);
        }

        private static Mock<IMessageTypeResolver> StubResolver<TMessage>(TimeSpan? expiresAfter)
            where TMessage : class
        {
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<TMessage>())
                    .Returns(new MessageBinding("urn:message:fake", typeof(TMessage), expiresAfter));
            return resolver;
        }

        internal class OutboundChannelWrapper : OutboundChannel
        {
            public OutboundChannelWrapper(IModel model)
                : base(model)
            {
            }

            internal void CallOnModelBasicAcks(BasicAckEventArgs args)
            {
                OnModelBasicAcks(null, args);
            }
        }
    }
}