using System;
using Carrot.Logging;
using RabbitMQ.Client;

namespace Carrot.Configuration
{
    public class EnvironmentConfiguration
    {
        internal EnvironmentConfiguration()
        {
            IdGenerator = new NewGuid();
            SerializationConfiguration = new SerializationConfiguration();
        }

        internal Uri EndpointUri { get; private set; }

        internal IMessageTypeResolver MessageTypeResolver { get; private set; } = new DefaultMessageTypeResolver();

        internal UInt32 PrefetchSize { get; private set; }

        internal UInt16 PrefetchCount { get; private set; }

        internal INewId IdGenerator { get; private set; }

        internal ILog Log { get; private set; } = new DefaultLog();

        internal SerializationConfiguration SerializationConfiguration { get; }

        internal Func<IModel, EnvironmentConfiguration, IOutboundChannel> OutboundChannelBuilder { get; private set; } = OutboundChannel.Default();

        public void Endpoint(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            EndpointUri = uri;
        }

        public void ResolveMessageTypeBy(IMessageTypeResolver resolver)
        {
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            MessageTypeResolver = resolver;
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
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            IdGenerator = instance;
        }

        public void LogBy(ILog log)
        {
            if (log == null)
                throw new ArgumentNullException(nameof(log));

            Log = log;
        }

        public void ConfigureSerialization(Action<SerializationConfiguration> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            configure(SerializationConfiguration);
        }

        public void PublishBy(Func<IModel, EnvironmentConfiguration, IOutboundChannel> builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            OutboundChannelBuilder = builder;
        }
    }
}