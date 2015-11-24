using System;
using System.Linq;
using System.Reflection;

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

        public MessageBinding Resolve(String source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

           var type = Type.GetType(source) ??
           _assemblies.Select(_ => _.GetType(source))
                      .FirstOrDefault(_ => _ != null);

            return type == null ? EmptyMessageBinding.Instance : new MessageBinding(source, type);
        }

        public MessageBinding Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage);
            return new MessageBinding(type.FullName, type);
        }
    }
}