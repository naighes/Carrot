using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RabbitMQ.Client;

namespace Carrot.Tests
{
    public static class BasicPropertiesStubber
    {
        public static IBasicProperties Stub(Action<IBasicProperties> configure = default)
        {
            var basicProperties = InstantiateViaReflection();
            configure?.Invoke(basicProperties);
            return basicProperties;
        }

        private static IBasicProperties InstantiateViaReflection()
        {
            var assembly = Assembly.Load("RabbitMQ.Client");
            var ctor = assembly
                .GetType("RabbitMQ.Client.Framing.BasicProperties")
                .GetConstructors().Single();
            return (IBasicProperties) ctor.Invoke(new object[0]);
        }
    }
    
}