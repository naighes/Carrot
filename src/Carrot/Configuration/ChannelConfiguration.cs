using System;

namespace Carrot.Configuration
{
    public class ChannelConfiguration
    {
        private readonly SerializationConfiguration _serializationConfiguration;

        internal ChannelConfiguration()
        {
            IdGenerator = new NewGuid();
            _serializationConfiguration = new SerializationConfiguration();
        }

        internal Uri EndpointUri { get; private set; }

        internal IMessageTypeResolver MessageTypeResolver { get; private set; }

        internal UInt32 PrefetchSize { get; private set; }

        internal UInt16 PrefetchCount { get; private set; }

        internal INewId IdGenerator { get; private set; }

        internal SerializationConfiguration SerializationConfiguration
        {
            get { return _serializationConfiguration; }
        }

        public void Endpoint(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            EndpointUri = uri;
        }

        public void ResolveMessageTypeBy(IMessageTypeResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException("resolver");

            MessageTypeResolver = resolver; // TODO: we should define a very basic resolving strategy, with a strong convention.
        }

        public void SetPrefetchSize(UInt32 value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("value");

            PrefetchSize = value;
        }

        public void SetPrefetchCount(UInt16 value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException("value");

            PrefetchCount = value;
        }

        public void GeneratesMessageIdBy(INewId instance)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            IdGenerator = instance;
        }

        public void ConfigureSerialization(Action<SerializationConfiguration> configure)
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            configure(_serializationConfiguration);
        }
    }
}