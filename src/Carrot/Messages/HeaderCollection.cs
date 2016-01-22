using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class HeaderCollection
    {
        private const string MessageIdKey = "message_id";
        private const string TimestampKey = "timestamp";
        internal const string ContentEncodingKey = "content_encoding";
        internal const string ContentTypeKey = "content_type";
        internal const string CorrelationIdKey = "correlation_id";
        internal const string ReplyToKey = "reply_to";

        protected internal readonly ISet<String> ReservedKeys = new HashSet<String>
                                                                    {
                                                                        MessageIdKey,
                                                                        TimestampKey,
                                                                        ContentTypeKey,
                                                                        ContentEncodingKey,
                                                                        CorrelationIdKey,
                                                                        ReplyToKey
                                                                    };

        internal readonly IDictionary<String, Object> InternalDictionary;

        internal HeaderCollection()
            : this(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase))
        {
        }

        internal HeaderCollection(IDictionary<String, Object> dictionary)
        {
            InternalDictionary = dictionary;
        }

        public String MessageId => ValueOrDefault<String>(MessageIdKey);

        public Int64 Timestamp => ValueOrDefault<Int64>(TimestampKey);

        public String ContentType => ValueOrDefault<String>(ContentTypeKey);

        public String ContentEncoding => ValueOrDefault<String>(ContentEncodingKey);

        public String CorrelationId => ValueOrDefault<String>(CorrelationIdKey);

        public String ReplyTo => ValueOrDefault<String>(ReplyToKey);

        public Object this[String key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                if (ReservedKeys.Contains(key))
                    throw new InvalidOperationException($"key '{key}' is reserved");

                return InternalDictionary[key];
            }
        }

        internal static HeaderCollection Parse(IBasicProperties properties)
        {
            var headers = new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase)
                              {
                                  { MessageIdKey, properties.MessageId },
                                  { TimestampKey, properties.Timestamp.UnixTime },
                                  { ContentTypeKey, properties.ContentType },
                                  { ContentEncodingKey, properties.ContentEncoding },
                                  { CorrelationIdKey, properties.CorrelationId },
                                  { ReplyToKey, properties.ReplyTo }
                              };

            if (properties.Headers != null)
                foreach (var header in properties.Headers)
                    if (!headers.ContainsKey(header.Key))
                        headers.Add(header.Key, header.Value);

            return new HeaderCollection(headers);
        }

        private T ValueOrDefault<T>(String key)
        {
            return InternalDictionary.ContainsKey(key) ? (T)InternalDictionary[key] : default(T);
        }

        internal void AddHeader(String key, Object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (ReservedKeys.Contains(key))
                throw new InvalidOperationException($"key '{key}' is reserved");

            InternalDictionary.Add(key, value);
        }

        internal void RemoveHeader(String key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (ReservedKeys.Contains(key))
                throw new InvalidOperationException($"key '{key}' is reserved");

            InternalDictionary.Remove(key);
        }

        internal IDictionary<String, Object> NonReservedHeaders()
        {
            return InternalDictionary.Where(_ => !ReservedKeys.Contains(_.Key))
                                     .ToDictionary(_ => _.Key, _ => _.Value);
        }

        internal void Set<T>(String key, T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (!InternalDictionary.ContainsKey(key))
                InternalDictionary.Add(key, value);
            else
                InternalDictionary[key] = value;
        }
    }
}