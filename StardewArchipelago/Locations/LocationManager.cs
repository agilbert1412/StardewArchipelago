using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations
{
    public class LocationManager
    {
        private static IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private IModHelper _modHelper;
        private Harmony _harmony;
        private LocationsCodeInjection _locationsCodeInjection;

        private Dictionary<string, long> _checkedLocations;

        public LocationManager(IMonitor monitor, ArchipelagoClient archipelago, BundleReader bundleReader, IModHelper modHelper, Harmony harmony, List<string> locationsAlreadyChecked)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _modHelper = modHelper;
            _harmony = harmony;
            _checkedLocations = locationsAlreadyChecked.ToDictionary(x => x, x => (long)-1);
            _locationsCodeInjection = new LocationsCodeInjection(_monitor, _modHelper, _archipelago, bundleReader, AddCheckedLocation);
        }

        public List<string> GetAllLocationsAlreadyChecked()
        {
            return _checkedLocations.Keys.ToList();
        }

        private void AddCheckedLocation(string locationName)
        {
            if (_checkedLocations.ContainsKey(locationName))
            {
                return;
            }

            var locationId = _archipelago.GetLocationId(locationName);

            if (locationId == -1)
            {
                _monitor.Log($"Location \"{locationName}\" could not be converted to an Archipelago id", LogLevel.Error);
            }

            _checkedLocations.Add(locationName, locationId);
            SendAllLocationChecks(false);
        }

        public void SendAllLocationChecks(bool forceReport = false)
        {
            if (!_archipelago.IsConnected)
            {
                return;
            }

            TryToIdentifyUnknownLocationNames();

            var allCheckedLocations = new List<long>();
            allCheckedLocations.AddRange(_checkedLocations.Values);

            allCheckedLocations = allCheckedLocations.Distinct().Where(x => x > -1).ToList();

            _archipelago.ReportCollectedLocations(allCheckedLocations.ToArray());
        }

        private void TryToIdentifyUnknownLocationNames()
        {
            foreach (var locationName in _checkedLocations.Keys)
            {
                if (_checkedLocations[locationName] > -1)
                {
                    continue;
                }

                var locationId = _archipelago.GetLocationId(locationName);
                if (locationId == -1)
                {
                    continue;
                }

                _checkedLocations[locationName] = locationId;
            }
        }

        public void ReplaceAllLocationsRewardsWithChecks()
        {
            RemoveDefaultRewardsOnAllBundles();
            ReplaceCommunityCenterBundlesWithChecks();
            ReplaceCommunityCenterAreasWithChecks();
            ReplaceBackPackUpgradesWithChecks();
            ReplaceMineshaftChestsWithChecks();
            ReplaceElevatorsWithChecks();
            ReplaceToolUpgradesWithChecks();
            ReplaceFishingRodsWithChecks();
        }

        private static void RemoveDefaultRewardsOnAllBundles()
        {
            foreach (var key in Game1.netWorldState.Value.BundleData.Keys)
            {
                var splitKey = key.Split('/');
                var value = Game1.netWorldState.Value.BundleData[key];
                var splitValue = value.Split('/');

                var areaName = splitKey[0];
                var bundleReward = splitValue[1];

                if (bundleReward == "")
                {
                    continue;
                }

                Game1.netWorldState.Value.BundleData[key] = value.Replace(bundleReward, "");
            }
        }

        private void ReplaceCommunityCenterBundlesWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(JunimoNoteMenu), nameof(JunimoNoteMenu.checkForRewards)),
                postfix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.CheckForRewards_PostFix))
            );
        }

        private void ReplaceCommunityCenterAreasWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "doAreaCompleteReward"),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.DoAreaCompleteReward_AreaLocations_Prefix))
            );
        }

        private void ReplaceBackPackUpgradesWithChecks()
        {
            if (_archipelago.SlotData.BackpackProgression == BackpackProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.PerformAction_BuyBackpack_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.AnswerDialogueAction_BackPackPurchase_Prefix))
            );

            // This would need a transpile patch for SeedShop.draw and I don't think it's worth it.
            // _harmony.Patch(
            //     original: AccessTools.Method(typeof(SeedShop), nameof(SeedShop.draw)),
            //     transpiler: new HarmonyMethod(typeof(BackpackInjections), nameof(LocationsCodeInjection.AnswerDialogueAction_BackPackPurchase_Prefix)));
        }

        private void ReplaceMineshaftChestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.CheckForAction_MineshaftChest_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.AddLevelChests_Level120_Prefix))
            );
        }

        private void ReplaceToolUpgradesWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(ScytheInjections), nameof(ScytheInjections.PerformAction_GoldenScythe_Prefix))
            );

            if (_archipelago.SlotData.ToolProgression == ToolProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(ToolInjections), nameof(ToolInjections.AnswerDialogueAction_ToolUpgrade_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Tool), nameof(Tool.actionWhenPurchased)),
                prefix: new HarmonyMethod(typeof(ToolInjections), nameof(ToolInjections.ActionWhenPurchased_ToolUpgrade_Prefix))
            );
        }

        private void ReplaceFishingRodsWithChecks()
        {
            if (_archipelago.SlotData.ToolProgression == ToolProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.SkipEvent_BambooPole_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_awardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.AwardFestivalPrize_BambooPole_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getFishShopStock)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.GetFishShopStock_Prefix))
            );
        }

        private void ReplaceElevatorsWithChecks()
        {
            if (_archipelago.SlotData.ElevatorProgression == ElevatorProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.EnterMine_SendElevatorCheck_PostFix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.PerformAction_LoadElevatorMenu_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.CheckAction_LoadElevatorMenu_Prefix))
            );
        }
    }
}
