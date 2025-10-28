using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class WalnutInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public void foundWalnut(int stack = 1)
        public static bool FoundWalnut_NoUpperLimit_Prefix(Farmer __instance, int stack)
        {
            try
            {
                Game1.netWorldState.Value.GoldenWalnuts += stack;
                Game1.netWorldState.Value.GoldenWalnutsFound += stack;
                Game1.PerformActionWhenPlayerFree(__instance.showNutPickup);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FoundWalnut_NoUpperLimit_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
