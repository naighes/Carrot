using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Carrot.Serialization
{
    public interface IContentNegotiator
    {
        SortedSet<ContentNegotiator.MediaTypeHeader> Negotiate(String contentType);
    }

    public class ContentNegotiator : IContentNegotiator
    {
        public SortedSet<MediaTypeHeader> Negotiate(String contentType)
        {
            return new SortedSet<MediaTypeHeader>(contentType.Split(new[] { ',' },
                                                                    StringSplitOptions.RemoveEmptyEntries)
                                                             .Select(_ => MediaTypeHeader.Parse(_.Trim()))
                                                             .OrderByDescending(_ => _.Quality),
                                                  MediaTypeHeader.MediaTypeHeaderQualityComparer.Instance);
        }

        public class MediaTypeHeader : IEquatable<MediaTypeHeader>
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
                return Equals(left, right);
            }

            public static Boolean operator !=(MediaTypeHeader left, MediaTypeHeader right)
            {
                return !Equals(left, right);
            }

            public Boolean Equals(MediaTypeHeader other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return Type.Equals(other.Type);
            }

            public override Boolean Equals(Object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                var other = obj as MediaTypeHeader;
                return other != null && Equals(other);
            }

            public override Int32 GetHashCode()
            {
                return Type.GetHashCode();
            }

            public override String ToString()
            {
                return String.Format("{0};q={1}", Type, Quality.ToString(CultureInfo.InvariantCulture));
            }

            public static MediaTypeHeader Parse(String source)
            {
                var type = default(MediaType);
                var quality = DefaultQuality;

                foreach (var s in source.Split(new[] { ';' },
                                               StringSplitOptions.RemoveEmptyEntries)
                                        .Select(_ => _.Trim()))
                    if (s.StartsWith("q", StringComparison.Ordinal))
                        quality = Single.Parse(s.Substring(s.IndexOf('=') + 1)
                                                .TrimStart(),
                                               CultureInfo.InvariantCulture);
                    else if (s.IndexOf('=') == -1) 
                        type = MediaType.Parse(s);

                return new MediaTypeHeader(type, quality);
            }

            internal class MediaTypeHeaderQualityComparer : IComparer<MediaTypeHeader>
            {
                internal static readonly MediaTypeHeaderQualityComparer Instance = new MediaTypeHeaderQualityComparer();

                private MediaTypeHeaderQualityComparer()
                {
                }

                public Int32 Compare(MediaTypeHeader x, MediaTypeHeader y)
                {
                    return x.Quality.CompareTo(y.Quality) * -1;
                }
            }
        }

        public abstract class RegistrationTree : IEquatable<RegistrationTree>
        {
            public readonly String Name;
            public readonly String Suffix;
            public readonly String Prefix;

            internal RegistrationTree(String name, String suffix, String prefix)
            {
                Name = name;
                Suffix = suffix;
                Prefix = prefix;
            }

            public static Boolean operator ==(RegistrationTree left, RegistrationTree right)
            {
                return Equals(left, right);
            }

            public static Boolean operator !=(RegistrationTree left, RegistrationTree right)
            {
                return !Equals(left, right);
            }

            public Boolean Equals(RegistrationTree other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return String.Equals(Prefix, other.Prefix) &&
                       String.Equals(Name, other.Name) &&
                       String.Equals(Suffix, other.Suffix);
            }

            public override Boolean Equals(Object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                var other = obj as RegistrationTree;
                return other != null && Equals(other);
            }

            public override Int32 GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ 
                           ((Prefix != null ? Prefix.GetHashCode() : 0) * 397) ^ 
                           (Suffix != null ? Suffix.GetHashCode() : 0);
                }
            }

            public override String ToString()
            {
                return Suffix == null
                    ? String.Format("{0}{1}", Prefix, Name)
                    : String.Format("{0}{1}+{2}", Prefix, Name, Suffix);
            }

            internal static RegistrationTree Parse(String source)
            {
                return source.StartsWith("vnd.", StringComparison.Ordinal)
                    ? new VendorTree(ParseName(source, "vnd."), ParseSuffix(source))
                    : (RegistrationTree)new StandardTree(ParseName(source), ParseSuffix(source));
            }

            protected static String ParseSuffix(String source)
            {
                const String key = "+";
                var start = source.IndexOf(key, StringComparison.Ordinal);

                return start == -1 ? null : source.Substring(start + 1);
            }

            protected static String ParseName(String source, String key = null)
            {
                var index = key == null ? 0 : source.IndexOf(key, StringComparison.Ordinal);

                if (index == -1)
                    return null;

                var start = index + (key == null ? 0 : key.Length);
                var end = source.IndexOf("+", StringComparison.Ordinal);

                return end == -1 ? source.Substring(start) : source.Substring(start, end - start);
            }
        }

        public class VendorTree : RegistrationTree
        {
            private const String PrefixKey = "vnd.";

            internal VendorTree(String name, String suffix)
                : base(name, suffix, PrefixKey)
            {
            }
        }

        public class StandardTree : RegistrationTree
        {
            private const String PrefixKey = "";

            internal StandardTree(String name, String suffix)
                : base(name, suffix, PrefixKey)
            {
            }
        }

        public class MediaType : IEquatable<MediaType>
        {
            public readonly String Type;
            public readonly RegistrationTree RegistrationTree;

            internal MediaType(String type, RegistrationTree registrationTree = null)
            {
                Type = type;
                RegistrationTree = registrationTree;
            }

            public static Boolean operator ==(MediaType left, MediaType right)
            {
                return Equals(left, right);
            }

            public static Boolean operator !=(MediaType left, MediaType right)
            {
                return !Equals(left, right);
            }

            public Boolean Equals(MediaType other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return String.Equals(Type, other.Type) && RegistrationTree.Equals(other.RegistrationTree);
            }

            public override Boolean Equals(Object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                var other = obj as MediaType;
                return other != null && Equals(other);
            }

            public override Int32 GetHashCode()
            {
                unchecked
                {
                    return (Type.GetHashCode() * 397) ^ RegistrationTree.GetHashCode();
                }
            }

            public override String ToString()
            {
                return RegistrationTree == null
                    ? Type
                    : String.Format("{0}/{1}", Type, RegistrationTree);
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

                return strings.Length <= 0 
                    ? new MediaType(type) 
                    : new MediaType(type, RegistrationTree.Parse(strings[1].Trim()));
            }
        }
    }
}