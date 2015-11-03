using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public class Queue : IEquatable<Queue>
    {
        private readonly String _name;
        private readonly Boolean _isDurable;

        private Queue(String name, Boolean isDurable)
        {
            _name = name;
            _isDurable = isDurable;
        }

        public String Name
        {
            get { return _name; }
        }

        public static Queue New(String name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            return new Queue(name, false);
        }

        public static Boolean operator ==(Queue left, Queue right)
        {
            return Equals(left, right);
        }

        public static Boolean operator !=(Queue left, Queue right)
        {
            return !Equals(left, right);
        }

        public Queue Durable()
        {
            return new Queue(Name, true);
        }

        public Boolean Equals(Queue other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return String.Equals(_name, other._name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Queue;
            return other != null && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return _name.GetHashCode();
        }

        internal void Declare(IModel model)
        {
            model.QueueDeclare(_name, _isDurable, false, false, new Dictionary<String, Object>());
        }
    }
}