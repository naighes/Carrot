using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Serialization;
using Xunit;

namespace Carrot.Tests
{
    public class SerializerBuilding
    {
        [Fact]
        public void JsonSerializer()
        {
            var serializerFactory = new SerializerFactory();
            var serializer = serializerFactory.Create("application/json");
            Assert.IsType<JsonSerializer>(serializer);
        }

        [Fact]
        public void CustomSerializer()
        {
            const String contentType = "application/fake";
            var serializerFactory = new SerializerFactory(new Dictionary<String, ISerializer>
                                                              {
                                                                  { contentType, new FakeSerializer() }
                                                              });
            var serializer = serializerFactory.Create(contentType);
            Assert.IsType<FakeSerializer>(serializer);
        }

        internal class FakeSerializer : ISerializer
        {
            public Object Deserialize(Byte[] body, Type type, Encoding encoding = null)
            {
                throw new NotImplementedException();
            }

            public String Serialize(Object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}