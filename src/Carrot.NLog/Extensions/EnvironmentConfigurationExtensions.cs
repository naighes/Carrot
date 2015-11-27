using Carrot.Configuration;
using global::NLog;

namespace Carrot.NLog.Extensions
{
    public static class EnvironmentConfigurationExtensions
    {
        public static void UseNLog(this EnvironmentConfiguration configuration, ILogger logger)
        {
            configuration.LogBy(new LogWrapper(logger));
        }
    }
}