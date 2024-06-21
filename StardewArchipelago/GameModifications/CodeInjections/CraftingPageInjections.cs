using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class CraftingPageInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }

        // public override void snapToDefaultClickableComponent()
        public static bool SnapToDefaultClickableComponent_DontCrashIfEmpty_Prefix(CraftingPage __instance)
        {
            try
            {
                
                if (__instance.pagesOfCraftingRecipes.Any() && __instance.pagesOfCraftingRecipes.First().Any())
                {
                    return true; // run original logic
                }

                __instance.currentlySnappedComponent = null;
                __instance.snapCursorToCurrentSnappedComponent();
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SnapToDefaultClickableComponent_DontCrashIfEmpty_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
