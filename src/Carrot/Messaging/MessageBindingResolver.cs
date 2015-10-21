using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carrot.Messaging
{
    public class MessageBindingResolver : IMessageTypeResolver
    {
        private readonly IDictionary<String, Type> _internalMap;

        public MessageBindingResolver(params Assembly[] assemblies)
        {
            _internalMap = assemblies.SelectMany(_ => _.GetTypes())
                                     .Where(_ => _.GetCustomAttribute<MessageBindingAttribute>(false) != null)
                                     .ToDictionary(_ => _.GetCustomAttribute<MessageBindingAttribute>(false)
                                                         .MessageType,
                                                   _ => _);
        }

        public MessageType Resolve(String source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return _internalMap.ContainsKey(source)
                       ? new MessageType(source, _internalMap[source])
                       : EmptyMessageType.Instance;
        }

        public MessageType Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage);
            var attribute = type.GetCustomAttribute<MessageBindingAttribute>();

            return attribute != null
                ? new MessageType(attribute.MessageType, type)
                : new MessageType(String.Format("urn:message:{0}", type.FullName), type);
        }
    }
}