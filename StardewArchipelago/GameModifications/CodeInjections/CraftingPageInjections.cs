using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
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
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __instance.currentlySnappedComponent = null;
                __instance.snapCursorToCurrentSnappedComponent();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SnapToDefaultClickableComponent_DontCrashIfEmpty_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
