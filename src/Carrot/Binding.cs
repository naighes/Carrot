using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public class Binding : IEquatable<Binding>
    {
        private readonly Queue _queue;
        private readonly String _routingKey;

        public Binding(Queue queue, String routingKey)
        {
            _queue = queue;
            _routingKey = routingKey;
        }

        public static Boolean operator ==(Binding left, Binding right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(Binding left, Binding right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(Binding other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _queue.Equals(other._queue) && String.Equals(_routingKey, other._routingKey);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Binding;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            unchecked
            {
                return (_queue.GetHashCode() * 397) ^ _routingKey.GetHashCode();
            }
        }

        internal void Declare(IModel model, Exchange exchange)
        {
            model.QueueBind(_queue.Name, exchange.Name, _routingKey, new Dictionary<String, Object>());
        }
    }
}