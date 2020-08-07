using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Configuration;
using Carrot.Extensions;
using Moq;
using Xunit;

namespace Carrot.Tests
{
    using RabbitMQ.Client;

    public class MessageProperties
    {
        [Fact]
        public void ReadingRabbitContentType()
        {
            const String contentType = "application/custom";
            var model = new Mock<IModel>();
            var properties = BasicPropertiesStubber.Stub();
            properties.ContentType = contentType;
            Assert.Equal(contentType, properties.ContentTypeOrDefault());
        }

        [Fact]
        public void HeadersFallbackContentType()
        {
            const String contentType = "application/custom";
            const String contentEncoding = "UTF-8";
            var encoding = Encoding.GetEncoding(contentEncoding);
            var properties = BasicPropertiesStubber.Stub();
            properties.ContentEncoding = contentEncoding;
            properties.Headers = new Dictionary<String, Object>
            {
                {"Content-Type", encoding.GetBytes(contentType)}
            };
            Assert.Equal(contentType, properties.ContentTypeOrDefault());
        }

        [Fact]
        public void DefaultContentType()
        {
            var properties = BasicPropertiesStubber.Stub();
            Assert.Equal(SerializationConfiguration.DefaultContentType,
                         properties.ContentTypeOrDefault());
        }

        [Fact]
        public void ReadingRabbitContentEncoding()
        {
            const String contentEncoding = "UTF-16";
            var properties = BasicPropertiesStubber.Stub();
            properties.ContentEncoding = contentEncoding;
            
            Assert.Equal(contentEncoding, properties.ContentEncodingOrDefault());
        }

        [Fact]
        public void DefaultContentEncoding()
        {
            var properties = BasicPropertiesStubber.Stub();
            Assert.Equal(SerializationConfiguration.DefaultContentEncoding,
                         properties.ContentEncodingOrDefault());
        }
    }
}