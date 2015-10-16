namespace Carrot.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class AppDomainAssembliesResolver : IMessageTypeResolver
    {
        private readonly IDictionary<String, Type> _internalMap;

        public AppDomainAssembliesResolver(params Assembly[] assemblies)
        {
            this._internalMap = assemblies.SelectMany(_ => _.GetTypes())
                                     .Where(_ => _.GetCustomAttribute<MessageBindingAttribute>(false) != null)
                                     .ToDictionary(_ => _.GetCustomAttribute<MessageBindingAttribute>(false)
                                                         .MessageType,
                                                   _ => _);
        }

        public MessageType Resolve(String source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            return this._internalMap.ContainsKey(source)
                       ? new MessageType(source, this._internalMap[source])
                       : EmptyMessageType.Instance;
        }
    }
}