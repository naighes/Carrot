using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Carrot.Serialization
{
    public class ContentNegotiator
    {
        public SortedSet<MediaTypeHeader> Negotiate(IBasicProperties properties)
        {
            return ParseContentType(properties.ContentType);
        }

        private static SortedSet<MediaTypeHeader> ParseContentType(String contentType)
        {
            return new SortedSet<MediaTypeHeader>(contentType.Split(new[] { ',' },
                                                                    StringSplitOptions.RemoveEmptyEntries)
                                                             .Select(_ => MediaTypeHeader.Parse(_.Trim()))
                                                             .OrderByDescending(_ => _.Quality),
                                                  MediaTypeHeader.MediaTypeHeaderQualityComparer.Instance);
        }

        public struct MediaTypeHeader
        {
            internal readonly MediaType Type;
            internal readonly Single Quality;

            private const Single DefaultQuality = 1.0f;

            internal MediaTypeHeader(MediaType type, Single quality)
            {
                Type = type;
                Quality = quality;
            }

            public static Boolean operator ==(MediaTypeHeader left, MediaTypeHeader right)
            {
                return left.Equals(right);
            }

            public static Boolean operator !=(MediaTypeHeader left, MediaTypeHeader right)
            {
                return !left.Equals(right);
            }

            public Boolean Equals(MediaTypeHeader other)
            {
                return Type == other.Type;
            }

            public override Boolean Equals(Object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                return obj is MediaTypeHeader && Equals((MediaTypeHeader)obj);
            }

            public override Int32 GetHashCode()
            {
                return Type.GetHashCode();
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

            internal class MediaTypeHeaderQualityComparer : IComparer<MediaTypeHeader>
            {
                internal static MediaTypeHeaderQualityComparer Instance = new MediaTypeHeaderQualityComparer();

                private MediaTypeHeaderQualityComparer()
                {
                }

                public Int32 Compare(MediaTypeHeader x, MediaTypeHeader y)
                {
                    return x.Quality.CompareTo(y.Quality) * -1;
                }
            }
        }

        public struct MediaType
        {
            public readonly String Type;
            public readonly String Vendor;
            public readonly String Suffix;

            internal MediaType(String type, String vendor = null, String suffix = null)
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

            public override String ToString()
            {
                return String.Format("{0}/vnd.{1}+{2}", Type, Vendor, Suffix);
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

                var strings = source.Split(new[] { '/' },
                                           StringSplitOptions.RemoveEmptyEntries)
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