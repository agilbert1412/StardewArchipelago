using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.GingerIsland.Parrots
{
    public class IslandLocationInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

        private IslandLocation _islandLocation;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_InteractWithParrots_Prefix(IslandLocation __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                foreach (var parrotUpgradePerch in __instance.parrotUpgradePerches)
                {
                    if (parrotUpgradePerch is not ParrotUpgradePerchArchipelago archipelagoParrot)
                    {
                        continue;
                    }

                    if (archipelagoParrot.CheckActionArchipelago(tileLocation))
                    {
                        __result = true;
                        return false; // don't run original logic
                    }
                }

                return true; // run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_InteractWithParrots_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
