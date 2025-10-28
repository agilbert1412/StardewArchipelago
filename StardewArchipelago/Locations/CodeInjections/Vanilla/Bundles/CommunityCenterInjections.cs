using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Goals;
using StardewArchipelago.Locations.Modded.SVE;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Locations;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants.Vanilla;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public static class CommunityCenterInjections
    {
        public const string AP_LOCATION_PANTRY = "Complete Pantry";
        public const string AP_LOCATION_CRAFTS_ROOM = "Complete Crafts Room";
        public const string AP_LOCATION_FISH_TANK = "Complete Fish Tank";
        public const string AP_LOCATION_BOILER_ROOM = "Complete Boiler Room";
        public const string AP_LOCATION_VAULT = "Complete Vault";
        public const string AP_LOCATION_BULLETIN_BOARD = "Complete Bulletin Board";
        public const string AP_LOCATION_ABANDONED_JOJA_MART = "The Missing Bundle";

        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static FriendshipReleaser _friendshipReleaser;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, LocationChecker locationChecker, BundleReader bundleReader)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _friendshipReleaser = new FriendshipReleaser(locationChecker, bundleReader);
        }

        public static bool DoAreaCompleteReward_AreaLocations_Prefix(CommunityCenter __instance, int whichArea)
        {
            try
            {
                var AreaAPLocationName = "";
                var mailToSend = "";
                switch ((Area)whichArea)
                {
                    case Area.Pantry:
                        AreaAPLocationName = AP_LOCATION_PANTRY;
                        mailToSend = "apccPantry";
                        break;
                    case Area.CraftsRoom:
                        AreaAPLocationName = AP_LOCATION_CRAFTS_ROOM;
                        mailToSend = "apccCraftsRoom";
                        break;
                    case Area.FishTank:
                        AreaAPLocationName = AP_LOCATION_FISH_TANK;
                        mailToSend = "apccFishTank";
                        break;
                    case Area.BoilerRoom:
                        AreaAPLocationName = AP_LOCATION_BOILER_ROOM;
                        mailToSend = "apccBoilerRoom";
                        break;
                    case Area.Vault:
                        AreaAPLocationName = AP_LOCATION_VAULT;
                        mailToSend = "apccVault";
                        break;
                    case Area.Bulletin:
                        AreaAPLocationName = AP_LOCATION_BULLETIN_BOARD;
                        mailToSend = "apccBulletin";
                        break;
                    case Area.AbandonedJojaMart:
                        AreaAPLocationName = AP_LOCATION_ABANDONED_JOJA_MART;
                        mailToSend = "apccMovieTheater";
                        break;
                }

                if (!Game1.player.mailReceived.Contains(mailToSend))
                {
                    Game1.player.mailForTomorrow.Add(mailToSend + "%&NL&%");
                }

                _locationChecker.AddCheckedLocation(AreaAPLocationName);
                _friendshipReleaser.ReleaseMorrisHeartsIfNeeded();
                GoalCodeInjection.CheckCommunityCenterGoalCompletion();

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoAreaCompleteReward_AreaLocations_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool ShouldNoteAppearInArea_AllowAccessEverything_Prefix(CommunityCenter __instance, int area, ref bool __result)
        {
            try
            {
                switch ((Area)area)
                {
                    case Area.Pantry:
                        __result = !Game1.player.hasOrWillReceiveMail("apccPantry"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_PANTRY);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case Area.CraftsRoom:
                        __result = !Game1.player.hasOrWillReceiveMail("apccCraftsRoom"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_CRAFTS_ROOM);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case Area.FishTank:
                        __result = !Game1.player.hasOrWillReceiveMail("apccFishTank"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_FISH_TANK);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case Area.BoilerRoom:
                        __result = !Game1.player.hasOrWillReceiveMail("apccBoilerRoom"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_BOILER_ROOM);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case Area.Vault:
                        __result = !Game1.player.hasOrWillReceiveMail("apccVault"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_VAULT);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case Area.Bulletin:
                        __result = !Game1.player.hasOrWillReceiveMail("apccBulletin"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_BULLETIN_BOARD);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case Area.AbandonedJojaMart:
                        __result = !Game1.player.hasOrWillReceiveMail("apccMovieTheater"); // _locationChecker.IsLocationNotChecked(AP_LOCATION_BULLETIN_BOARD);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                __result = false;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShouldNoteAppearInArea_AllowAccessEverything_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool CheckAction_BulletinBoardNoRequirements_Prefix(CommunityCenter __instance,
            Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tile = __instance.map.GetLayer("Buildings").Tiles[tileLocation];
                if (tile == null || tile.TileIndex != 1799)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __instance.checkBundle(5);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_BulletinBoardNoRequirements_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // private void checkForMissedRewards()
        public static bool CheckForMissedRewards_DontBother_Prefix(CommunityCenter __instance)
        {
            try
            {
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForMissedRewards_DontBother_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // protected override void resetSharedState()
        public static void ResetSharedState_SisyphusStoneFallDown_Postfix(CommunityCenter __instance)
        {
            try
            {
                if (ArchipelagoJunimoNoteMenu.SisyphusStoneNeedsToFall)
                {
                    foreach (var (bundleKey, bundleData) in Game1.netWorldState.Value.BundleData)
                    {
                        var bundleFields = bundleData.Split("/");
                        var bundleName = bundleFields[0];
                        if (bundleName != MemeBundleNames.SISYPHUS)
                        {
                            continue;
                        }

                        var areaName = bundleKey.Split("/")[0];
                        var area = CommunityCenter.getAreaNumberFromName(areaName);
                        var notePosition = GetNotePosition(area);
                        notePosition.Y += 1;
                        var noteVector = new Vector2(notePosition.X, notePosition.Y) * 64f;

                        var item = ItemRegistry.Create(QualifiedItemIds.STONE);
                        Game1.createItemDebris(item, noteVector, 0, __instance);
                        ArchipelagoJunimoNoteMenu.SisyphusStoneNeedsToFall = false;
                        return;
                    }

                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetSharedState_SisyphusStoneFallDown_Postfix)}:\n{ex}");
                return;
            }
        }

        private static Point GetNotePosition(int area)
        {
            switch (area)
            {
                case 0:
                    return new Point(14, 5);
                case 1:
                    return new Point(14, 23);
                case 2:
                    return new Point(40, 10);
                case 3:
                    return new Point(63, 14);
                case 4:
                    return new Point(55, 6);
                case 5:
                    return new Point(46, 11);
                default:
                    return Point.Zero;
            }
        }

        // public void markAreaAsComplete(int area)
        public static bool MarkAreaAsComplete_SkipIllegalAreas_Prefix(CommunityCenter __instance, int area)
        {
            try
            {
                if (area >= __instance.areasComplete.Count)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MarkAreaAsComplete_SkipIllegalAreas_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

    }
}
