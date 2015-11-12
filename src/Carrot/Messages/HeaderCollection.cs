using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Carrot.Messages
{
    public class HeaderCollection
    {
        private readonly ISet<String> _reserverKeys = new HashSet<String>
                                                          {
                                                              "message_id",
                                                              "timestamp"
                                                          };

        private readonly IDictionary<String, Object> _dictionary;

        internal HeaderCollection()
            : this(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase))
        {
        }

        internal HeaderCollection(IDictionary<String, Object> dictionary)
        {
            _dictionary = dictionary;
        }

        public String MessageId
        {
            get { return _dictionary["message_id"] as String; }
        }

        public Int64 Timestamp
        {
            get { return (Int64)_dictionary["timestamp"]; }
        }

        public Object this[String key]
        {
            get
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                if (_reserverKeys.Contains(key))
                    throw new InvalidOperationException(String.Format("key '{0}' is reserved", key));

                return _dictionary[key];
            }
        }

        public void AddHeader(String key, Object value)
        {
            if (_reserverKeys.Contains(key))
                throw new InvalidOperationException(String.Format("key '{0}' is reserved", key));

            _dictionary.Add(key, value);
        }

        public void RemoveHeader(String key)
        {
            if (_reserverKeys.Contains(key))
                throw new InvalidOperationException(String.Format("key '{0}' is reserved", key));

            _dictionary.Remove(key);
        }

        internal static HeaderCollection Parse(BasicDeliverEventArgs args)
        {
            return new HeaderCollection(new Dictionary<String, Object>(StringComparer.OrdinalIgnoreCase)
                                            {
                                                { "message_id", args.BasicProperties.MessageId },
                                                { "timestamp", args.BasicProperties.Timestamp.UnixTime }
                                            });
        }

        internal void HydrateProperties(IBasicProperties properties)
        {
            properties.Persistent = false;

            foreach (var pair in _dictionary)
                if (!_reserverKeys.Contains(pair.Key))
                    properties.Headers.Add(pair.Key, pair.Value);
        }
    }
}