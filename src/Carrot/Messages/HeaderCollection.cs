using System;
using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Carrot.Messages
{
    public class HeaderCollection
    {
        protected internal readonly ISet<String> ReservedKeys = new HashSet<String>
                                                                    {
                                                                        "message_id",
                                                                        "timestamp",
                                                                        "content_type",
                                                                        "content_encoding",
                                                                        "correlation_id",
                                                                        "reply_to"
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

        public String MessageId => ValueOrDefault<String>("message_id");

        public Int64 Timestamp => ValueOrDefault<Int64>("timestamp");

        public String ContentType => ValueOrDefault<String>("content_type");

        public String ContentEncoding => ValueOrDefault<String>("content_encoding");

        public String CorrelationId => ValueOrDefault<String>("correlation_id");

        public String ReplyTo => ValueOrDefault<String>("reply_to");

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
                                  { "message_id", properties.MessageId },
                                  { "timestamp", properties.Timestamp.UnixTime },
                                  { "content_type", properties.ContentType },
                                  { "content_encoding", properties.ContentEncoding },
                                  { "correlation_id", properties.CorrelationId },
                                  { "reply_to", properties.ReplyTo }
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
    }
}