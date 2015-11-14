using System;
using System.Text;
using Newtonsoft.Json;

namespace Carrot.Serialization
{
    public class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings();

        public JsonSerializerSettings Settings
        {
            get { return _settings; }
        }

        public Object Deserialize(Byte[] body, Type type, Encoding encoding = null)
        {
            var e = encoding ?? new UTF8Encoding(true);
            return JsonConvert.DeserializeObject(e.GetString(body), type, _settings);
        }

        public String Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}