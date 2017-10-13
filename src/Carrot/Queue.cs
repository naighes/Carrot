using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public struct Queue
    {
        public readonly String Name;
        internal readonly Boolean IsDurable;
        internal readonly IDictionary<String, Object> Arguments;

        internal Queue(String name,
                       Boolean isDurable = false,
                       IDictionary<String, Object> arguments = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            IsDurable = isDurable;
            Arguments = arguments ?? new Dictionary<String, Object>();
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

            return obj is Queue queue && Equals(queue);
        }

        public override Int32 GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal void Declare(IModel model)
        {
            model.QueueDeclare(Name,
                               IsDurable,
                               false,
                               false,
                               Arguments);
        }
    }
}