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
    public class CommunityCenterInjections
    {
        private static IMonitor _monitor;
        private static BundleReader _bundleReader;
        private static Action<string> _addCheckedLocation;

        public CommunityCenterInjections(IMonitor monitor, BundleReader bundleReader, Action<string> addCheckedLocation)
        {
                _monitor = monitor;
            _bundleReader = bundleReader;
            _addCheckedLocation = addCheckedLocation;
        }

        public static bool DoAreaCompleteReward_AreaLocations_Prefix(CommunityCenter __instance, int whichArea)
        {
            try
            {
                var AreaAPLocationName = "";
                switch ((Area)whichArea)
                {
                    case Area.Pantry:
                        AreaAPLocationName = "Complete Pantry";
                        break;
                    case Area.CraftsRoom:
                        AreaAPLocationName = "Complete Crafts Room";
                        break;
                    case Area.FishTank:
                        AreaAPLocationName = "Complete Fish Tank";
                        break;
                    case Area.BoilerRoom:
                        AreaAPLocationName = "Complete Boiler Room";
                        break;
                    case Area.Vault:
                        AreaAPLocationName = "Complete Vault";
                        break;
                    case Area.Bulletin:
                        AreaAPLocationName = "Complete Bulletin Board";
                        break;
                }

                _addCheckedLocation(AreaAPLocationName);
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
                    _addCheckedLocation(completedBundleName);
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
