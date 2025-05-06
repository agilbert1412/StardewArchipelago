using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Locations;
using StardewModdingAPI;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Locations.InGameLocations;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class ScoutInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void exitThisMenu(bool playSound = true)
        public static bool ExitThisMenu_ScoutShopContent_Prefix(IClickableMenu __instance, bool playSound)
        {
            try
            {
                if (__instance is not ShopMenu shopMenu)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var locationsToScout = new List<string>();
                foreach (var (item, stockInfo) in shopMenu.itemPriceAndStock)
                {
                    if (item is not ObtainableArchipelagoLocation apItem || stockInfo.Stock == 0)
                    {
                        continue;
                    }

                    locationsToScout.Add(apItem.LocationName);
                }

                if (!locationsToScout.Any())
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _archipelago.ScoutStardewLocations(locationsToScout, true);

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ExitThisMenu_ScoutShopContent_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
