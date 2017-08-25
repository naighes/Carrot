using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public class ExchangeBinding : IEquatable<ExchangeBinding>
    {
        private readonly Exchange _exchange;
        private readonly Queue _queue;
        private readonly String _routingKey;
        private readonly IDictionary<String, Object> _arguments;

        public ExchangeBinding(Exchange exchange,
                               Queue queue,
                               String routingKey,
                               IDictionary<String, Object> arguments = null)
        {
            _exchange = exchange;
            _queue = queue;
            _routingKey = routingKey;
            _arguments = arguments ?? new Dictionary<String, Object>();
        }

        public static Boolean operator ==(ExchangeBinding left, ExchangeBinding right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(ExchangeBinding left, ExchangeBinding right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(ExchangeBinding other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _exchange == other._exchange &&
                   _queue == other._queue &&
                   String.Equals(_routingKey, other._routingKey);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as ExchangeBinding;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                var hashCode = _exchange.GetHashCode();
                hashCode = (hashCode * 397) ^ _queue.GetHashCode();
                hashCode = (hashCode * 397) ^ _routingKey.GetHashCode();
                return hashCode;
            }
        }

        internal void Declare(IModel model)
        {
            model.QueueBind(_queue.Name,
                            _exchange.Name,
                            _routingKey,
                            _arguments);
        }
    }
}