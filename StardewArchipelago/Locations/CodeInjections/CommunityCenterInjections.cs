using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Goals;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class CommunityCenterInjections
    {
        public const string AP_LOCATION_PANTRY = "Complete Pantry";
        public const string AP_LOCATION_CRAFTS_ROOM = "Complete Crafts Room";
        public const string AP_LOCATION_FISH_TANK = "Complete Fish Tank";
        public const string AP_LOCATION_BOILER_ROOM = "Complete Boiler Room";
        public const string AP_LOCATION_VAULT = "Complete Vault";
        public const string AP_LOCATION_BULLETIN_BOARD = "Complete Bulletin Board";

        private static IMonitor _monitor;
        private static BundleReader _bundleReader;
        private static LocationChecker _locationChecker;

        public static Dictionary<string, string> _bundleNames;

        public static void Initialize(IMonitor monitor, BundleReader bundleReader, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _bundleReader = bundleReader;
            _locationChecker = locationChecker;
            _bundleNames = new Dictionary<string, string>();
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
                }

                if (!Game1.player.mailReceived.Contains(mailToSend))
                {
                    Game1.player.mailForTomorrow.Add(mailToSend + "%&NL&%");
                }
                _locationChecker.AddCheckedLocation(AreaAPLocationName);
                GoalCodeInjection.CheckCommunityCenterGoalCompletion();

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DoAreaCompleteReward_AreaLocations_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void CheckForRewards_PostFix(JunimoNoteMenu __instance)
        {
            try
            {
                var bundleStates = _bundleReader.ReadCurrentBundleStates();
                var completedBundleNames = bundleStates.Where(x => x.IsCompleted).Select(x => x.RelatedBundle.BundleName);
                foreach (var completedBundleName in completedBundleNames)
                {
                    var bundleNameLocation = completedBundleName;
                    if (_bundleNames.ContainsKey(bundleNameLocation))
                    {
                        bundleNameLocation = _bundleNames[bundleNameLocation];
                    }
                    _locationChecker.AddCheckedLocation(bundleNameLocation + " Bundle");
                }

                var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
                var bundleRewardsDictionary = communityCenter.bundleRewards;
                foreach (var bundleRewardKey in bundleRewardsDictionary.Keys)
                {
                    bundleRewardsDictionary[bundleRewardKey] = false;
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForRewards_PostFix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool ShouldNoteAppearInArea_AllowAccessEverything_Prefix(CommunityCenter __instance, int area, ref bool __result)
        {
            try
            {
                switch ((Area)area)
                {
                    case Area.Pantry:
                        __result = !Game1.player.hasOrWillReceiveMail("apccPantry"); // _locationChecker.IsLocationMissing(AP_LOCATION_PANTRY);
                        return false; // don't run original logic
                    case Area.CraftsRoom:
                        __result = !Game1.player.hasOrWillReceiveMail("apccCraftsRoom"); // _locationChecker.IsLocationMissing(AP_LOCATION_CRAFTS_ROOM);
                        return false; // don't run original logic
                    case Area.FishTank:
                        __result = !Game1.player.hasOrWillReceiveMail("apccFishTank"); // _locationChecker.IsLocationMissing(AP_LOCATION_FISH_TANK);
                        return false; // don't run original logic
                    case Area.BoilerRoom:
                        __result = !Game1.player.hasOrWillReceiveMail("apccBoilerRoom"); // _locationChecker.IsLocationMissing(AP_LOCATION_BOILER_ROOM);
                        return false; // don't run original logic
                    case Area.Vault:
                        __result = !Game1.player.hasOrWillReceiveMail("apccVault"); // _locationChecker.IsLocationMissing(AP_LOCATION_VAULT);
                        return false; // don't run original logic
                    case Area.Bulletin:
                        __result = !Game1.player.hasOrWillReceiveMail("apccBulletin"); // _locationChecker.IsLocationMissing(AP_LOCATION_BULLETIN_BOARD);
                        return false; // don't run original logic
                    case Area.AbandonedJojaMart:
                        if (Utility.HasAnyPlayerSeenEvent(191393))
                        {
                            __result = true;
                            return false; // don't run original logic
                        }

                        break;
                }
                __result = false;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShouldNoteAppearInArea_AllowAccessEverything_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
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
                    return true; // run original logic
                }

                __instance.checkBundle(5);
                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_BulletinBoardNoRequirements_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
