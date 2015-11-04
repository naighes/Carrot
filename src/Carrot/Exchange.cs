using System;
using System.Collections.Generic;
using Carrot.Messages;
using RabbitMQ.Client;

namespace Carrot
{
    public class Exchange : IEquatable<Exchange>
    {
        internal readonly String Type;
        internal readonly String Name;
        internal readonly Boolean IsDurable;

        private readonly IDictionary<Queue, String> _bindings = new Dictionary<Queue, String>();

        private Exchange(String name, String type, Boolean isDurable = false)
        {
            Type = type;
            IsDurable = isDurable;
            Name = name;
        }

        public static Exchange Direct(String name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return new Exchange(name, "direct");
        }

        public static Exchange Fanout(String name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return new Exchange(name, "fanout");
        }

        public static Exchange Topic(String name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return new Exchange(name, "topic");
        }

        public static Exchange Headers(String name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return new Exchange(name, "headers");
        }

        public static Boolean operator ==(Exchange left, Exchange right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(Exchange left, Exchange right)
        {
            return !Equals(left, right);
        }

        public Boolean Equals(Exchange other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return String.Equals(Name, other.Name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Exchange;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return Name.GetHashCode();
        }

        public Exchange Durable()
        {
            return new Exchange(Name, Type, true);
        }

        public void Bind(Queue queue, String routingKey = "")
        {
            _bindings.Add(queue, routingKey);
        }

        internal void Declare(IModel model, IConsumedMessageBuilder builder)
        {
            model.ExchangeDeclare(Name, Type, IsDurable, false, new Dictionary<String, Object>());

            foreach (var binding in _bindings)
            {
                var queue = binding.Key;
                queue.Declare(model, builder);
                model.QueueBind(queue.Name, Name, binding.Value, new Dictionary<String, Object>());
            }
        }
    }
}