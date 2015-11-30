using System;
using Carrot.Configuration;
using Carrot.Extensions;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public struct ConsumedMessageContext
    {
        public readonly String Source;
        public readonly String MessageType;
        public readonly String ContentType;
        public readonly String ContentEncoding;

        internal ConsumedMessageContext(String source,
                                        String messageType,
                                        String contentType,
                                        String contentEncoding)
        {
            Source = source;
            MessageType = messageType;
            ContentType = contentType;
            ContentEncoding = contentEncoding;
        }

        public static Boolean operator ==(ConsumedMessageContext left, ConsumedMessageContext right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(ConsumedMessageContext left, ConsumedMessageContext right)
        {
            return !left.Equals(right);
        }

        public static ConsumedMessageContext FromBasicDeliverEventArgs(BasicDeliverEventArgs args)
        {
            var source = args.Exchange;
            var contentType = args.BasicProperties.ContentTypeOrDefault(SerializationConfiguration.DefaultContentType);
            var contentEncoding = args.BasicProperties.ContentEncodingOrDefault(SerializationConfiguration.DefaultContentEncoding);
            var messageType = args.BasicProperties.Type;
            return new ConsumedMessageContext(source, messageType, contentType, contentEncoding);
        }

        public Boolean Equals(ConsumedMessageContext other)
        {
            return String.Equals(Source, other.Source) &&
                   String.Equals(MessageType, other.MessageType) &&
                   String.Equals(ContentType, other.ContentType) &&
                   String.Equals(ContentEncoding, other.ContentEncoding);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is ConsumedMessageContext && Equals((ConsumedMessageContext)obj);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                var hashCode = Source?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (MessageType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ContentType?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ContentEncoding?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}