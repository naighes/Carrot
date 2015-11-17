using Carrot.Configuration;
using global::NLog;

namespace Carrot.NLog.Extensions
{
    public static class ChannelConfigurationExtensions
    {
        public static void UseNLog(this ChannelConfiguration configuration, ILogger logger)
        {
            configuration.LogBy(new LogWrapper(logger));
        }
    }
}