using System;
using System.Linq;
using System.Reflection;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        internal static readonly IMessageTypeResolver Instance = new DefaultMessageTypeResolver(AppDomain.CurrentDomain.GetAssemblies());
        private readonly Assembly[] _assemblies;

        public DefaultMessageTypeResolver(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public MessageBinding Resolve(ConsumedMessageContext context)
        {
            var messageType = context.MessageType;
            var type = Type.GetType(messageType) ??
           _assemblies.Select(_ => _.GetType(messageType))
                      .FirstOrDefault(_ => _ != null);

            return type == null
                       ? EmptyMessageBinding.Instance
                       : new MessageBinding(messageType, type.GetTypeInfo());
        }

        public MessageBinding Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage).GetTypeInfo();

            return new MessageBinding(type.FullName, type);
        }
    }
}