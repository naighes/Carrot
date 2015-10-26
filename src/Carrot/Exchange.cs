using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Carrot
{
    public struct Exchange
    {
        internal readonly String Type;
        internal readonly String Name;
        internal readonly Boolean IsDurable;

        internal Exchange(String name, String type, Boolean isDurable = false)
        {
            Type = type;
            IsDurable = isDurable;
            Name = name;
        }

        public static Boolean operator ==(Exchange left, Exchange right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(Exchange left, Exchange right)
        {
            return !left.Equals(right);
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

        public Exchange Durable()
        {
            return new Exchange(Name, Type, true);
        }

        public Boolean Equals(Exchange other)
        {
            return String.Equals(Name, other.Name);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Exchange && Equals((Exchange)obj);
        }

        public override Int32 GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal void Declare(IModel model)
        {
            model.ExchangeDeclare(Name, Type, IsDurable);
        }

        internal void Bind(MessageQueue queue, IModel model, String routingKey = "")
        {
            model.QueueBind(queue.Name, Name, routingKey, new Dictionary<String, Object>());
        }
    }
}