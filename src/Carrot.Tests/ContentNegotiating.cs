using System;
using System.Linq;
using Carrot.Serialization;
using RabbitMQ.Client.Framing;
using Xunit;

namespace Carrot.Tests
{
    public class ContentNegotiating
    {
        [Fact]
        public void ContentTypeParsing()
        {
            const String input = "text/plain; q=0.5, text/html, text/x-dvi; q=0.8,application/vnd.checkmate+json;version=2;q=0.1";
            var types = new ContentNegotiator().Negotiate(new BasicProperties { ContentType = input });

            var first = types.First();
            Assert.Equal(ContentNegotiator.MediaType.Parse("text/html"), first.Type);
            Assert.Equal(1.0f, first.Quality);

            var second = types.Skip(1).First();
            Assert.Equal(ContentNegotiator.MediaType.Parse("text/x-dvi"), second.Type);
            Assert.Equal(0.8f, second.Quality);

            var third = types.Skip(2).First();
            Assert.Equal(ContentNegotiator.MediaType.Parse("text/plain"), third.Type);
            Assert.Equal(0.5f, third.Quality);

            var fourth = types.Skip(3).First();
            var mediaType = ContentNegotiator.MediaType.Parse("application/vnd.checkmate+json");
            Assert.Equal(mediaType, fourth.Type);
            Assert.Equal(0.1f, fourth.Quality);

            Assert.Equal("checkmate", mediaType.Vendor);
            Assert.Equal("json", mediaType.Suffix);
        }
    }
}