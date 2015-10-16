using System;

namespace TowerBridge.Common.Infrastructure.Messaging
{
    public class MessageType : IEquatable<MessageType>
    {
        public readonly String RawName;
        public readonly Type RuntimeType;

        internal MessageType(String rawName, Type runtimeType)
        {
            RawName = rawName;
            RuntimeType = runtimeType;
        }

        public static Boolean operator ==(MessageType left, MessageType right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(MessageType left, MessageType right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(MessageType other)
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

            var other = obj as MessageType;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return RawName != null ? RawName.GetHashCode() : 0;
        }
    }
}