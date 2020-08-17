using System;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Carrot.Serialization
{
    public class JsonSerializer : ISerializer
    {
        public JsonSerializerSettings Settings { get; } = new JsonSerializerSettings();

        public object Deserialize(ReadOnlyMemory<Byte> body, TypeInfo type, Encoding encoding = null)
        {
            var e = encoding ?? new UTF8Encoding(true);
#if NETCOREAPP3_1
            
            var json = e.GetString(body.Span);
#else
            var json = e.GetString(body);
#endif      
            return JsonConvert.DeserializeObject(json, type.AsType(), Settings);
        }

        public String Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        
    }
}