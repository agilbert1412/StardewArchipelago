﻿using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class CraftingPageInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
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
                _logger.LogError($"Failed in {nameof(SnapToDefaultClickableComponent_DontCrashIfEmpty_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
