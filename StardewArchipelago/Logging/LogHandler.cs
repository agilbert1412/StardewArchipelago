using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;

namespace StardewArchipelago.Logging
{
    public class LogHandler : Logger
    {
        private readonly IMonitor _logger;

        public LogHandler(IMonitor logger)
        {
            _logger = logger;
        }

        public override void LogError(string message)
        {
            _logger.Log(message, LogLevel.Error);
        }

        public override void LogWarning(string message)
        {
            _logger.Log(message, LogLevel.Warn);
        }

        public override void LogInfo(string message)
        {
            _logger.Log(message, LogLevel.Info);
        }

        public override void LogMessage(string message)
        {
            _logger.Log(message, LogLevel.Alert);
        }

        public override void LogDebug(string message)
        {
            _logger.Log(message, LogLevel.Trace);
        }

        public void Log(string message, LogLevel logLevel)
        {
            _logger.Log(message, logLevel);
        }
    }
}
