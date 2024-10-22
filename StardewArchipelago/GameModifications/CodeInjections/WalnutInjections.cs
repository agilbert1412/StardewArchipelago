using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

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
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(FoundWalnut_NoUpperLimit_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
