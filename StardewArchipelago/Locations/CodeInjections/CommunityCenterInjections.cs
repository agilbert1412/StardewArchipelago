using System;
using System.Linq;
using StardewArchipelago.Goals;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

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

        public static void Initialize(IMonitor monitor, BundleReader bundleReader, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _bundleReader = bundleReader;
            _locationChecker = locationChecker;
        }

        public static bool DoAreaCompleteReward_AreaLocations_Prefix(CommunityCenter __instance, int whichArea)
        {
            try
            {
                var AreaAPLocationName = "";
                switch ((Area)whichArea)
                {
                    case Area.Pantry:
                        AreaAPLocationName = AP_LOCATION_PANTRY;
                        break;
                    case Area.CraftsRoom:
                        AreaAPLocationName = AP_LOCATION_CRAFTS_ROOM;
                        break;
                    case Area.FishTank:
                        AreaAPLocationName = AP_LOCATION_FISH_TANK;
                        break;
                    case Area.BoilerRoom:
                        AreaAPLocationName = AP_LOCATION_BOILER_ROOM;
                        break;
                    case Area.Vault:
                        AreaAPLocationName = AP_LOCATION_VAULT;
                        break;
                    case Area.Bulletin:
                        AreaAPLocationName = AP_LOCATION_BULLETIN_BOARD;
                        break;
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
                var completedBundleNames = bundleStates.Where(x => x.IsCompleted).Select(x => x.RelatedBundle.BundleName + " Bundle");
                foreach (var completedBundleName in completedBundleNames)
                {
                    _locationChecker.AddCheckedLocation(completedBundleName);
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
    }
}
