using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Carrot.Configuration;
using Carrot.Serialization;
using Moq;
using Xunit;

namespace Carrot.Tests
{
    public class ContentNegotiating
    {
        [Fact]
        public void ContentTypeParsing()
        {
            const String contentType = "text/plain; q=0.5, text/html, text/x-dvi; q=0.8,application/vnd.checkmate+json;version=2;q=0.1";
            var types = new ContentNegotiator().Negotiate(contentType);

            var first = types.First();
            Assert.Equal(ContentNegotiator.MediaType.Parse("text/html"), first.MediaType);
            Assert.Equal(1.0f, first.Quality);

            var second = types.Skip(1).First();
            Assert.Equal(ContentNegotiator.MediaType.Parse("text/x-dvi"), second.MediaType);
            Assert.Equal(0.8f, second.Quality);

            var third = types.Skip(2).First();
            Assert.Equal(ContentNegotiator.MediaType.Parse("text/plain"), third.MediaType);
            Assert.Equal(0.5f, third.Quality);

            var fourth = types.Skip(3).First();
            var mediaType = ContentNegotiator.MediaType.Parse("application/vnd.checkmate+json");
            Assert.Equal(mediaType, fourth.MediaType);
            Assert.Equal(0.1f, fourth.Quality);
        }

        [Fact]
        public void CustomMap()
        {
            const String contentType = "application/dummy";
            var configuration = new SerializationConfiguration();
            configuration.Map(header => header.MediaType == contentType, new FakeSerializer());
            var serializer = configuration.Create(contentType);
            Assert.IsType<FakeSerializer>(serializer);
        }

        [Fact]
        public void DefaultSerializer()
        {
            const String contentType = "application/json";
            var configuration = new SerializationConfiguration();
            var serializer = configuration.Create(contentType);
            Assert.IsType<JsonSerializer>(serializer);
        }

        [Fact]
        public void NotFound()
        {
            const String contentType = "application/unknow";
            var configuration = new SerializationConfiguration();
            var serializer = configuration.Create(contentType);
            Assert.IsType<NullSerializer>(serializer);
        }

        [Fact]
        public void CustomNegotiation()
        {
            const String contentType = "application/custom";
            var configuration = new SerializationConfiguration();
            configuration.Map(_ => _.MediaType == "application/custom", new FakeSerializer());
            var negotiator = new Mock<IContentNegotiator>();
            var @set = new SortedSet<ContentNegotiator.MediaTypeHeader>();
            @set.Add(ContentNegotiator.MediaTypeHeader.Parse(contentType));
            negotiator.Setup(_ => _.Negotiate(contentType))
                      .Returns(@set);
            configuration.NegotiateBy(negotiator.Object);
            var serializer = configuration.Create(contentType);
            Assert.IsType<FakeSerializer>(serializer);
        }

        internal class FakeSerializer : ISerializer
        {
            public Object Deserialize(ReadOnlyMemory<Byte> body,
                                      TypeInfo type,
                                      Encoding encoding = null)
            {
                throw new NotImplementedException();
            }

            public String Serialize(Object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}