using Carrot.Serialization;
using Xunit;

namespace Carrot.Tests
{
    public class MediaTypeParsing
    {
        [Fact]
        public void NoTreeNoSuffix()
        {
            var header = ContentNegotiator.MediaTypeHeader.Parse("application/xml");
            Assert.Equal(1.0f, header.Quality);
            Assert.Equal("application/xml;q=1", header.ToString());
        }

        [Fact]
        public void StandardTree()
        {
            var header = ContentNegotiator.MediaTypeHeader.Parse("application/checkmate+json; q=0.2");
            Assert.Equal(0.2f, header.Quality);
            Assert.Equal("application/checkmate+json;q=0.2", header.ToString());
        }

        [Fact]
        public void VendorTree()
        {
            var header = ContentNegotiator.MediaTypeHeader.Parse("application/vnd.checkmate+json; q   = 0.3");
            Assert.Equal(0.3f, header.Quality);
            Assert.Equal("application/vnd.checkmate+json;q=0.3", header.ToString());
        }

        [Fact]
        public void VendorTreeNoSuffix()
        {
            var header = ContentNegotiator.MediaTypeHeader.Parse("application/vnd.checkmate; q   = 0.4");
            Assert.Equal(0.4f, header.Quality);
            Assert.Equal("application/vnd.checkmate;q=0.4", header.ToString());
        }

        [Fact]
        public void MediaTypeEquality()
        {
            var a = ContentNegotiator.MediaType.Parse("application/vnd.checkmate+json");
            var b = ContentNegotiator.MediaType.Parse("application/vnd.checkmate+xml");
            Assert.NotEqual(a, b);
            Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
            var c = ContentNegotiator.MediaType.Parse("application/vnd.checkmate+json");
            Assert.Equal(a, c);
            Assert.Equal(a.GetHashCode(), c.GetHashCode());
            var d = ContentNegotiator.MediaType.Parse("application/vnd.checkmate");
            Assert.NotEqual(a, d);
            Assert.NotEqual(a.GetHashCode(), d.GetHashCode());
            var e = ContentNegotiator.MediaType.Parse("application/checkmate+json");
            Assert.NotEqual(a, e);
            Assert.NotEqual(a.GetHashCode(), e.GetHashCode());

            var f = ContentNegotiator.MediaType.Parse("application/json");
            var g = ContentNegotiator.MediaType.Parse("application/json");
            Assert.Equal(f, g);
            Assert.Equal(f.GetHashCode(), g.GetHashCode());
            var h = ContentNegotiator.MediaType.Parse("application/xml");
            Assert.NotEqual(f, h);
            Assert.NotEqual(f.GetHashCode(), h.GetHashCode());
        }

        [Fact]
        public void MediaTypeHeaderEquality()
        {
            var mt1 = ContentNegotiator.MediaType.Parse("application/json");
            var mt2 = ContentNegotiator.MediaType.Parse("application/xml");
            var a = new ContentNegotiator.MediaTypeHeader(mt1, 0.1f);
            var b = new ContentNegotiator.MediaTypeHeader(mt1, 0.2f);
            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            var c = new ContentNegotiator.MediaTypeHeader(mt2, 0.1f);
            Assert.NotEqual(a, c);
            Assert.NotEqual(a.GetHashCode(), c.GetHashCode());
        }
    }
}