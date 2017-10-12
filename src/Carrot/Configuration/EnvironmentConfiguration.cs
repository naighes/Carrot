using System;
using Carrot.Logging;
using RabbitMQ.Client;

namespace Carrot.Configuration
{
    public class EnvironmentConfiguration
    {
        private IMessageTypeResolver resolver;

        internal EnvironmentConfiguration()
        {
            IdGenerator = new NewGuid();
            SerializationConfiguration = new SerializationConfiguration();
        }

        internal Uri EndpointUri { get; private set; }

        internal IMessageTypeResolver MessageTypeResolver
        {
            get => resolver ?? DefaultMessageTypeResolver.Instance;
            private set => resolver = value;
        }

        internal UInt32 PrefetchSize { get; private set; }

        internal UInt16 PrefetchCount { get; private set; }

        internal INewId IdGenerator { get; private set; }

        internal ILog Log { get; private set; } = new DefaultLog();

        internal SerializationConfiguration SerializationConfiguration { get; }

        internal Func<IModel, EnvironmentConfiguration, IOutboundChannel> OutboundChannelBuilder { get; private set; } = OutboundChannel.Default();

        public void Endpoint(Uri uri)
        {
            EndpointUri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        public void ResolveMessageTypeBy(IMessageTypeResolver messageTypeResolver)
        {
            MessageTypeResolver = messageTypeResolver ?? throw new ArgumentNullException(nameof(messageTypeResolver));
        }

        public void SetPrefetchSize(UInt32 value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            PrefetchSize = value;
        }

        public void SetPrefetchCount(UInt16 value)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            PrefetchCount = value;
        }

        public void GeneratesMessageIdBy(INewId instance)
        {
            IdGenerator = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        public void LogBy(ILog log)
        {
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void ConfigureSerialization(Action<SerializationConfiguration> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            configure(SerializationConfiguration);
        }

        public void PublishBy(Func<IModel, EnvironmentConfiguration, IOutboundChannel> builder)
        {
            OutboundChannelBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }
    }
}