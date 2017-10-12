using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Carrot.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public JsonSerializerSettings Settings { get; } = new JsonSerializerSettings();

        public Object Deserialize(Byte[] body, TypeInfo type, Encoding encoding = null)
        {
            var e = encoding ?? new UTF8Encoding(true);
            return JsonConvert.DeserializeObject(e.GetString(body),
                                                 type.AsType(),
                                                 Settings);
        }

        public String Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}