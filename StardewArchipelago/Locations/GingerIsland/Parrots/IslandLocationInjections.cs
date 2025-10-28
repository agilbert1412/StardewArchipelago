using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.GingerIsland.Parrots
{
    public class IslandLocationInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;

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
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_InteractWithParrots_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
