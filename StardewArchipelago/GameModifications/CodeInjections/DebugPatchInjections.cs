using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewValley;
using System;
using System.Diagnostics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class DebugPatchInjections
    {
        private const bool DebugStackTraces = true;

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        private static string GetRelevantLogs()
        {
            var canMoveLog = $"Farmer.CanMove is '{Game1.player.CanMove}'.";
            var eventUpLog = $"Game1.eventUp is '{Game1.eventUp}'.";
            var currentEventLog = $"Game1.CurrentEvent is '{Game1.CurrentEvent}'.";
            var stack = GetShortStackTrace();
            return $"{Environment.NewLine}\t{canMoveLog}" +
                   $"{Environment.NewLine}\t{eventUpLog}" +
                   $"{Environment.NewLine}\t{currentEventLog}" +
                   $"{Environment.NewLine}\t{stack}";
        }

        private static string GetShortStackTrace()
        {
#pragma warning disable CS0162
            if (!DebugStackTraces)
            {
                return string.Empty;
            }
#pragma warning restore CS0162

#pragma warning disable CS0162
            var stackTrace = new StackTrace();
            var stackTraceString = stackTrace.ToString();
            var thingsWorthStoppingFor = new[]
            {
                "System.Threading.Tasks", "StardewValley.Game1._update",
            };
            foreach (var thingWorthStoppingFor in thingsWorthStoppingFor)
            {
                if (!stackTraceString.Contains(thingWorthStoppingFor))
                {
                    continue;
                }

                var index = stackTraceString.IndexOf(thingWorthStoppingFor);
                stackTraceString = stackTraceString.Substring(0, index);
            }

            var stackLog = $"{Environment.NewLine}Stack Trace:{Environment.NewLine}\t{stackTraceString}";
            return stackLog;
#pragma warning restore CS0162
        }
    }
}
