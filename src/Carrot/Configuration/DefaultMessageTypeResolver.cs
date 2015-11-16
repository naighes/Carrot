using System;
using System.Linq;

namespace Carrot.Configuration
{
    public class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        public MessageBinding Resolve(String source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

           var type = Type.GetType(source) ??
           AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(source))
                    .FirstOrDefault(t => t != null);

            if (type == null)
                return EmptyMessageBinding.Instance;

            return new MessageBinding(source, type);
        }

        public MessageBinding Resolve<TMessage>() where TMessage : class
        {
            var type = typeof(TMessage);
            return new MessageBinding(type.FullName, type);
        }
    }
}