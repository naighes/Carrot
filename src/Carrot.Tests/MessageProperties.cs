using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class MessageProperties
    {
        [Fact]
        public void ReadingRabbitContentType()
        {
            const String contentType = "application/custom";
            var properties = new BasicProperties
                                 {
                                     ContentType = contentType
                                 };
            Assert.Equal(contentType, properties.ContentTypeOrDefault());
        }

        [Fact]
        public void HeadersFallbackContentType()
        {
            const String contentType = "application/custom";
            const String contentEncoding = "UTF-8";
            var encoding = Encoding.GetEncoding(contentEncoding);
            var properties = new BasicProperties
                                 {
                                     ContentEncoding = contentEncoding,
                                     Headers = new Dictionary<String, Object>
                                                   {
                                                       { "Content-Type", encoding.GetBytes(contentType) }
                                                   }
                                 };
            Assert.Equal(contentType, properties.ContentTypeOrDefault());
        }

        [Fact]
        public void DefaultContentType()
        {
            var properties = new BasicProperties();
            Assert.Equal(SerializationConfiguration.DefaultContentType,
                         properties.ContentTypeOrDefault());
        }

        [Fact]
        public void ReadingRabbitContentEncoding()
        {
            const String contentEncoding = "UTF-16";
            var properties = new BasicProperties
                                 {
                                     ContentEncoding = contentEncoding
                                 };
            Assert.Equal(contentEncoding, properties.ContentEncodingOrDefault());
        }

        [Fact]
        public void DefaultContentEncoding()
        {
            var properties = new BasicProperties();
            Assert.Equal(SerializationConfiguration.DefaultContentEncoding,
                         properties.ContentEncodingOrDefault());
        }
    }
}