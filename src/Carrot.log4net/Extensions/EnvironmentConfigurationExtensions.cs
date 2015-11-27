using Carrot.Configuration;
using global::log4net;

namespace Carrot.log4net.Extensions
{
    public static class EnvironmentConfigurationExtensions
    {
        public static void UseLog4Net(this EnvironmentConfiguration configuration, ILog log)
        {
            configuration.LogBy(new LogWrapper(log));
        }
    }
}