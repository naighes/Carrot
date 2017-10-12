using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public struct Exchange
    {
        public readonly String Name;
        internal readonly String Type;
        internal readonly Boolean IsDurable;

        internal Exchange(String name, String type, Boolean isDurable = false)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsDurable = isDurable;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public static Boolean operator ==(Exchange left, Exchange right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(Exchange left, Exchange right)
        {
            return !left.Equals(right);
        }

        public Boolean Equals(Exchange other)
        {
            return String.Equals(Name, other.Name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Exchange exchange && Equals(exchange);
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