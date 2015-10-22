using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Carrot.Serialization;
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
        }

        [Fact]
        public void CustomMap()
        {
            const String contentType = "application/dummy";
            var map = new Dictionary<String, ISerializer>
                          {
                              { contentType, new FakeSerializer() }
                          };
            var factory = new SerializerFactory(map);
            var serializer = factory.Create(contentType);
            Assert.IsType<FakeSerializer>(serializer);
        }

        [Fact]
        public void DefaultSerializer()
        {
            const String contentType = "application/json";
            var factory = new SerializerFactory();
            var serializer = factory.Create(contentType);
            Assert.IsType<JsonSerializer>(serializer);
        }

        [Fact]
        public void NotFound()
        {
            const String contentType = "application/unknow";
            var factory = new SerializerFactory();
            var serializer = factory.Create(contentType);
            Assert.IsType<NullSerializer>(serializer);
        }

        internal class FakeSerializer : ISerializer
        {
            public object Deserialize(Byte[] body, Type type, Encoding encoding = null)
            {
                throw new NotImplementedException();
            }

            public string Serialize(Object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}