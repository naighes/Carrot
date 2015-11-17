using Carrot.Configuration;
using global::log4net;

namespace Carrot.log4net.Extensions
{
    public static class ChannelConfigurationExtensions
    {
        public static void UseLog4Net(this ChannelConfiguration configuration, ILog log)
        {
            configuration.LogBy(new LogWrapper(log));
        }
    }
}