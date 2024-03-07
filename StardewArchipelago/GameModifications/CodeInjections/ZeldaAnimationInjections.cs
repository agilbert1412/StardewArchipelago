using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Extensions;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class ZeldaAnimationInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }

        // public void holdUpItemThenMessage(Item item, bool showMessage = true)
        public static bool HoldUpItemThenMessage_SkipBasedOnConfig_Prefix(Farmer __instance, Item item, bool showMessage)
        {
            try
            {
                // We skip this whole method when skipping hold up animations is true
                return !ModEntry.Instance.Config.SkipHoldUpAnimations;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(HoldUpItemThenMessage_SkipBasedOnConfig_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
