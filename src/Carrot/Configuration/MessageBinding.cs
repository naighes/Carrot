using System;

namespace Carrot.Configuration
{
    public class MessageBinding : IEquatable<MessageBinding>
    {
        public readonly String RawName;
        public readonly Type RuntimeType;
        public readonly TimeSpan? ExpiresAfter;

        public MessageBinding(String rawName, Type runtimeType)
            : this(rawName, runtimeType, null)
        {
        }

        internal MessageBinding(String rawName, Type runtimeType, TimeSpan? expiresAfter)
        {
            RawName = rawName;
            RuntimeType = runtimeType;
            ExpiresAfter = expiresAfter;
        }

        public static Boolean operator ==(MessageBinding left, MessageBinding right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(MessageBinding left, MessageBinding right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(MessageBinding other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return String.Equals(RawName, other.RawName);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as MessageBinding;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return RawName?.GetHashCode() ?? 0;
        }
    }
}