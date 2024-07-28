using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations;
using StardewModdingAPI;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class DebugPatchInjections
    {
        private const bool DebugStackTraces = true;

        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }

        // public bool CanMove
        public static bool CanMoveSet_DebugWtfIsHappening_Prefix(Farmer __instance, bool value)
        {
            try
            {
                _monitor.Log($"Setting {nameof(Farmer.CanMove)} to {value}:{GetRelevantLogs()}", LogLevel.Debug);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CanMoveSet_DebugWtfIsHappening_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public static void NewDay(float timeToPause)
        public static bool NewDay_DebugWtfIsHappening_Prefix(float timeToPause)
        {
            try
            {
                _monitor.Log($"About to run {nameof(Game1.NewDay)}:{GetRelevantLogs()}", LogLevel.Debug);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewDay_DebugWtfIsHappening_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
        public static void NewDay_DebugWtfIsHappening_Postfix(float timeToPause)
        {
            try
            {
                _monitor.Log($"Finished running {nameof(Game1.NewDay)}:{GetRelevantLogs()}", LogLevel.Debug);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewDay_DebugWtfIsHappening_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // private static IEnumerator<int> _newDayAfterFade()
        public static bool NewDayAfterFade_DebugWtfIsHappening_Prefix(ref IEnumerator<int> __result)
        {
            try
            {
                _monitor.Log($"About to run _newDayAfterFade:{GetRelevantLogs()}", LogLevel.Debug);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewDayAfterFade_DebugWtfIsHappening_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
        public static void NewDayAfterFade_DebugWtfIsHappening_Postfix(ref IEnumerator<int> __result)
        {
            try
            {
                _monitor.Log($"Finished running _newDayAfterFade:{GetRelevantLogs()}", LogLevel.Debug);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(NewDayAfterFade_DebugWtfIsHappening_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // private static void onFadedBackInComplete()
        public static bool OnFadedBackInComplete_DebugWtfIsHappening_Prefix()
        {
            try
            {
                _monitor.Log($"About to run onFadedBackInComplete:{GetRelevantLogs()}", LogLevel.Debug);
                return true; // run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(OnFadedBackInComplete_DebugWtfIsHappening_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
        public static void OnFadedBackInComplete_DebugWtfIsHappening_Postfix()
        {
            try
            {
                _monitor.Log($"Finished running onFadedBackInComplete:{GetRelevantLogs()}", LogLevel.Debug);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(OnFadedBackInComplete_DebugWtfIsHappening_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
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
            if (!DebugStackTraces)
            {
                return string.Empty;
            }
            
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
