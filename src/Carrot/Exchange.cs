using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public class Exchange : IEquatable<Exchange>
    {
        internal readonly String Type;
        internal readonly String Name;
        internal readonly Boolean IsDurable;

        internal Exchange(String name, String type, Boolean isDurable = false)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (type == null)
                throw new ArgumentNullException("type");
            
            Type = type;
            IsDurable = isDurable;
            Name = name;
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

        internal void Declare(IModel model)
        {
            model.ExchangeDeclare(Name, Type, IsDurable, false, new Dictionary<String, Object>());
        }
    }
}