using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carrot.Configuration
{
    public class MessageBindingResolver : IMessageTypeResolver
    {
        private readonly IDictionary<String, Tuple<Type, Int32>> _internalMap;

        public MessageBindingResolver(params Assembly[] assemblies)
        {
            _internalMap = assemblies.SelectMany(_ => _.GetTypes())
                                     .Where(_ => _.GetCustomAttribute<MessageBindingAttribute>(true) != null)
                                     .ToDictionary(_ => _.GetCustomAttribute<MessageBindingAttribute>(false)
                                                         .MessageType,
                                                   _ => new Tuple<Type, Int32>(_,
                                                                               _.GetCustomAttribute<MessageBindingAttribute>(false).ExpiresAfter));
        }

        public MessageBinding Resolve(String source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return _internalMap.ContainsKey(source)
                       ? BuildMessageBinding(source, _internalMap[source].Item1, _internalMap[source].Item2)
                       : EmptyMessageBinding.Instance;
        }

        public MessageBinding Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage);
            var attribute = type.GetCustomAttribute<MessageBindingAttribute>();

            return BuildMessageBinding(attribute != null ? attribute.MessageType : String.Format("urn:message:{0}", type.FullName),
                                       type,
                                       attribute != null ? attribute.ExpiresAfter : -1);
        }

        private static MessageBinding BuildMessageBinding(String source, Type type, Int32 expiresAfter)
        {
            return new MessageBinding(source,
                                      type,
                                      expiresAfter == -1 ? null : new TimeSpan?(TimeSpan.FromSeconds(expiresAfter)));
        }
    }
}