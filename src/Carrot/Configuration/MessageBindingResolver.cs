using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class MessageBindingResolver : IMessageTypeResolver
    {
        private readonly IDictionary<String, Tuple<TypeInfo, Int32>> _internalMap;

        public MessageBindingResolver(params Assembly[] assemblies)
        {
            _internalMap = assemblies.SelectMany(_ => _.GetTypes())
                                     .Select(_ => _.GetTypeInfo())
                                     .Where(_ => _.GetCustomAttribute<MessageBindingAttribute>(true) != null)
                                     .ToDictionary(_ => _.GetCustomAttribute<MessageBindingAttribute>(false)
                                                         .MessageType,
                                                   _ => new Tuple<TypeInfo, Int32>(_,
                                                                                   _.GetCustomAttribute<MessageBindingAttribute>(false).ExpiresAfter));
        }

        public MessageBinding Resolve(ConsumedMessageContext context)
        {
            return _internalMap.ContainsKey(context.MessageType)
                       ? BuildMessageBinding(context.MessageType,
                                             _internalMap[context.MessageType].Item1,
                                             _internalMap[context.MessageType].Item2)
                       : EmptyMessageBinding.Instance;
        }

        public MessageBinding Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage).GetTypeInfo();
            var attribute = type.GetCustomAttribute<MessageBindingAttribute>();

            return BuildMessageBinding(attribute != null
                                           ? attribute.MessageType
                                           : $"urn:message:{type.FullName}",
                                       type,
                                       attribute?.ExpiresAfter ?? -1);
        }

        private static MessageBinding BuildMessageBinding(String source, TypeInfo type, Int32 expiresAfter)
        {
            return new MessageBinding(source,
                                      type,
                                      expiresAfter == -1 ? null : new TimeSpan?(TimeSpan.FromSeconds(expiresAfter)));
        }
    }
}