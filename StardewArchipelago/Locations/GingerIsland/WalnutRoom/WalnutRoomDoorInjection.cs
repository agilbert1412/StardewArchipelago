using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.GingerIsland.WalnutRoom
{
    public class WalnutRoomDoorInjection
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_WalnutRoomDoorBasedOnAPItem_Prefix(IslandWest __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (__instance.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") == -1 ||
                    __instance.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") != 1470)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (_archipelago.HasReceivedItem(VanillaUnlockManager.QI_WALNUT_ROOM))
                {
                    Game1.playSound("doorClose");
                    Game1.warpFarmer("QiNutRoom", 7, 8, 0);
                }
                else
                {
                    // var walnutWarning = "You hear a strange voice from behind the door...#'Only the greatest walnut hunters may enter here.'^    Your current status: {0}/100";
                    var walnutWarningAP = "You hear a strange voice from behind the door...#'Usually, only the greatest walnut hunters may enter here, but your case is a bit special, isn't it? I believe a friend can help you out.'";
                    Game1.drawObjectDialogue(walnutWarningAP);
                }

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_WalnutRoomDoorBasedOnAPItem_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
    }
}
