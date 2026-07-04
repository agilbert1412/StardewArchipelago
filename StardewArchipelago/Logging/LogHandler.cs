using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewModdingAPI;
using System;
using System.Linq;

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

            // No longer necessary, now the patch is in ErrorInjections.cs to catch non-Archipelago errors as well
            //if (IsErrorRelatedToArchipelagoConnection(message))
            //{
            //    return;
            //}

            //ArchipelagoJunimoNoteMenu.CompleteBundleIfExists(MemeBundleNames.ERROR);
        }

        public static bool IsErrorRelatedToArchipelagoConnection(string message)
        {
            var invalidErrors = new[] { "Failed to Connect", "Connection to Archipelago", "closed the WebSocket connection" };
            return invalidErrors.Any(x => message.Contains(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public override void LogError(string message, Exception e)
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
