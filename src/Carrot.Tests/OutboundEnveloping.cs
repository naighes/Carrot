using System;
using Carrot.Configuration;
using Carrot.Extensions;
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

            var wrapper = new OutboundMessageEnvelope<Foo>(message, dateTimeProvider.Object, 0uL, configuration);
            var result = Assert.IsType<SuccessfulPublishing>(wrapper.PublishAsync(new OutboundChannel(model.Object),
                                                                                  new Exchange("target_exchange", "direct")).Result);
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
                _.Map(__ => __.MediaType == "application/json", serializer.Object);
            });

            var wrapper = new OutboundMessageEnvelope<Foo>(message, dateTimeProvider.Object, 0uL, configuration);
            var result = Assert.IsType<FailurePublishing>(wrapper.PublishAsync(new OutboundChannel(model.Object),
                                                                               new Exchange(exchange, "direct")).Result);
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
            var properties = message.BuildBasicProperties(configuration, dateTimeProvider.Object);
            Assert.Equal(messageId, properties.MessageId);
            Assert.Equal(timestamp.ToUnixTimestamp(), properties.Timestamp.UnixTime);
        }

        [Fact]
        public void MessageType()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var configuration = StubChannelConfiguration(StubResolver<Bar>(null));
            var properties = message.BuildBasicProperties(configuration, new Mock<IDateTimeProvider>().Object);
            Assert.Equal("urn:message:fake", properties.Type);
        }

        // TODO: overriding content encoding
        //[Fact]
        //public void ContentEncoding()
        //{
        //    const String contentEncoding = "UTF-16";
        //    var message = new OutboundMessage<Bar>(new Bar());
        //    var envelope = BuildDefaultEnvelope(message);
        //    var properties = new BasicProperties { ContentEncoding = contentEncoding };

        //    envelope.CallHydrateProperties(properties,
        //                                   message,
        //                                   new ChannelConfiguration(),
        //                                   new Mock<IDateTimeProvider>().Object);
        //    Assert.Equal(contentEncoding, properties.ContentEncoding);
        //}

        [Fact]
        public void DefaultContentEncoding()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var configuration = new ChannelConfiguration();
            var properties = message.BuildBasicProperties(configuration, new Mock<IDateTimeProvider>().Object);
            Assert.Equal("UTF-8", properties.ContentEncoding);
        }

        // TODO: overriding content type
        //[Fact]
        //public void ContentType()
        //{
        //    const String contentType = "application/xml";
        //    var message = new OutboundMessage<Bar>(new Bar());
        //    var envelope = BuildDefaultEnvelope(message);
        //    var properties = new BasicProperties { ContentType = contentType };
        //    envelope.CallHydrateProperties(properties,
        //                                   message,
        //                                   new ChannelConfiguration(),
        //                                   new Mock<IDateTimeProvider>().Object);
        //    Assert.Equal(contentType, properties.ContentType);
        //}

        [Fact]
        public void DefaultContentType()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var configuration = new ChannelConfiguration();
            var properties = message.BuildBasicProperties(configuration, new Mock<IDateTimeProvider>().Object);
            Assert.Equal("application/json", properties.ContentType);
        }

        [Fact]
        public void NonDurableMessage()
        {
            var message = new OutboundMessage<Bar>(new Bar());
            var configuration = new ChannelConfiguration();
            var properties = message.BuildBasicProperties(configuration, new Mock<IDateTimeProvider>().Object);
            Assert.False(properties.Persistent);
        }

        [Fact]
        public void DurableMessage()
        {
            var message = new DurableOutboundMessage<Bar>(new Bar());
            var configuration = new ChannelConfiguration();
            var properties = message.BuildBasicProperties(configuration, new Mock<IDateTimeProvider>().Object);
            Assert.True(properties.Persistent);
        }

        [Fact]
        public void MessageExpiration()
        {
            var expiresAfter = new TimeSpan?(TimeSpan.FromSeconds(18));
            var message = new DurableOutboundMessage<Bar>(new Bar());
            var configuration = StubChannelConfiguration(StubResolver<Bar>(expiresAfter));
            var properties = message.BuildBasicProperties(configuration, new Mock<IDateTimeProvider>().Object);
            Assert.Equal("18000", properties.Expiration);
        }

        private static ChannelConfiguration StubChannelConfiguration(IMock<IMessageTypeResolver> resolver)
        {
            var configuration = new ChannelConfiguration();
            configuration.ResolveMessageTypeBy(resolver.Object);
            configuration.GeneratesMessageIdBy(new Mock<INewId>().Object);
            return configuration;
        }

        private static Mock<IMessageTypeResolver> StubResolver<TMessage>(TimeSpan? expiresAfter)
            where TMessage : class
        {
            var resolver = new Mock<IMessageTypeResolver>();
            resolver.Setup(_ => _.Resolve<TMessage>())
                    .Returns(new MessageBinding("urn:message:fake", typeof(TMessage), expiresAfter));
            return resolver;
        }
    }
}