using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;
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
            var types = new ContentNegotiator().Negotiate(new BasicProperties { ContentType = input })
                                               .OrderByDescending(_ => _.Quality);

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

    public class ContentNegotiator
    {
        public IEnumerable<MediaTypeHeader> Negotiate(IBasicProperties properties)
        {
            return properties.ContentType
                             .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(_ => MediaTypeHeader.Parse(_.Trim()));
        }

        public struct MediaTypeHeader
        {
            private const Single DefaultQuality = 1.0f;

            internal readonly MediaType Type;
            internal readonly Single Quality;

            private MediaTypeHeader(MediaType type, Single quality)
            {
                Type = type;
                Quality = quality;
            }

            internal static MediaTypeHeader Parse(String source)
            {
                var type = default(MediaType);
                var quality = DefaultQuality;

                foreach (var s in source.Split(new[] { ';' },
                                               StringSplitOptions.RemoveEmptyEntries)
                                        .Select(_ => _.Trim()))
                    if (s.StartsWith("q", StringComparison.Ordinal))
                        quality = Single.Parse(s.Substring(s.IndexOf('=') + 1)
                                                .TrimStart());
                    else if (s.IndexOf('=') == -1) 
                        type = MediaType.Parse(s);

                return new MediaTypeHeader(type, quality);
            }
        }

        public struct MediaType
        {
            public readonly String Type;
            public readonly String Vendor;
            public readonly String Suffix;

            private MediaType(String type, String vendor = null, String suffix = null)
            {
                Type = type;
                Vendor = vendor;
                Suffix = suffix;
            }

            public static Boolean operator ==(MediaType left, MediaType right)
            {
                return left.Equals(right);
            }

            public static Boolean operator !=(MediaType left, MediaType right)
            {
                return !left.Equals(right);
            }

            public Boolean Equals(MediaType other)
            {
                return String.Equals(Type, other.Type) && 
                       String.Equals(Vendor, other.Vendor) && 
                       String.Equals(Suffix, other.Suffix);
            }

            public override Boolean Equals(Object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                return obj is MediaType && Equals((MediaType)obj);
            }

            public override Int32 GetHashCode()
            {
                unchecked
                {
                    var hashCode = Type != null ? Type.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (Vendor != null ? Vendor.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Suffix != null ? Suffix.GetHashCode() : 0);
                    return hashCode;
                }
            }

            internal static MediaType Parse(String source)
            {
                if (source == null)
                    throw new ArgumentNullException("source");

                var strings = source.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(_ => _.Trim())
                                    .ToArray();

                var type = strings[0];

                if (strings.Length <= 0)
                    return new MediaType(type);

                return new MediaType(type, 
                                     ParseSegment(strings[1].Trim(), "vnd.", "+"), 
                                     ParseSegment(strings[1].Trim(), "+"));
            }

            private static String ParseSegment(String source, String startKey, String endKey = null)
            {
                var index = source.IndexOf(startKey, StringComparison.Ordinal);

                if (index == -1)
                    return null;

                var start = index + startKey.Length;

                if (endKey == null)
                    return source.Substring(start, source.Length - start);

                var end = source.IndexOf(endKey, start, StringComparison.Ordinal);

                return end != -1 ? source.Substring(start, end - start) : null;
            }
        }
    }
}