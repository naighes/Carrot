using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public struct Queue
    {
        public readonly String Name;
        internal readonly Boolean IsDurable;

        internal Queue(String name, Boolean isDurable = false)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            Name = name;
            IsDurable = isDurable;
        }

        public static Boolean operator ==(Queue left, Queue right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(Queue left, Queue right)
        {
            return !left.Equals(right);
        }

        public Boolean Equals(Queue other)
        {
            return String.Equals(Name, other.Name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Queue && Equals((Queue)obj);
        }

        public override Int32 GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal void Declare(IModel model)
        {
            model.QueueDeclare(Name, IsDurable, false, false, new Dictionary<String, Object>());
        }
    }
}