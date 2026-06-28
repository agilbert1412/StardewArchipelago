using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using StardewArchipelago.Bundles;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    internal class ErrorInjections
    {
        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // private void LogImpl(string source, string message, ConsoleLogLevel level)
        public static void LogImpl_CompleteErrorBundleOnError_Postfix(object __instance, string source, string message, LogLevel level)
        {
            try
            {
                if (level < LogLevel.Error || LogHandler.IsErrorRelatedToArchipelagoConnection(message))
                {
                    return;
                }

                ArchipelagoJunimoNoteMenu.CompleteBundleIfExists(MemeBundleNames.ERROR);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(LogImpl_CompleteErrorBundleOnError_Postfix)}\t{ex}");
                return;
            }
        }
    }
}
