using System;
using System.Linq;
using System.Reflection;
using Carrot.Messages;

namespace Carrot.Configuration
{
    public class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        private readonly Assembly[] _assemblies;

        public DefaultMessageTypeResolver()
            : this(AppDomain.CurrentDomain.GetAssemblies())
        {
        }

        public DefaultMessageTypeResolver(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public MessageBinding Resolve(ConsumedMessageContext context)
        {
            var messageType = context.MessageType;
            var type = Type.GetType(messageType) ??
           _assemblies.Select(_ => _.GetType(messageType))
                      .FirstOrDefault(_ => _ != null);

            return type == null ? EmptyMessageBinding.Instance : new MessageBinding(messageType, type);
        }

        public MessageBinding Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage);
            return new MessageBinding(type.FullName, type);
        }
    }
}