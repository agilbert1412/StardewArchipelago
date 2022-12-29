using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Netcode;
using StardewArchipelago.Archipelago;
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

        private Dictionary<string, long> _reportedLocations;
        private Dictionary<string, long> _newLocations;

        public LocationManager(IMonitor monitor, ArchipelagoClient archipelago, BundleReader bundleReader, IModHelper modHelper, Harmony harmony)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _modHelper = modHelper;
            _harmony = harmony;
            _reportedLocations = new Dictionary<string, long>();
            _newLocations = new Dictionary<string, long>();
            _locationsCodeInjection = new LocationsCodeInjection(_monitor, _modHelper, _archipelago, bundleReader, AddCheckedLocation);
        }

        private void AddCheckedLocation(string locationName)
        {
            var locationId = _archipelago.GetLocationId(locationName);

            if (locationId == -1)
            {
                _monitor.Log($"Location \"{locationName}\" could not be converted to an Archipelago id", LogLevel.Error);
                _reportedLocations.Add(locationName, locationId);
                return;
            }

            _newLocations.Add(locationName, locationId);
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
            allCheckedLocations.AddRange(_reportedLocations.Values);
            allCheckedLocations.AddRange(_newLocations.Values);

            allCheckedLocations = allCheckedLocations.Distinct().Where(x => x > -1).ToList();
            if (forceReport || allCheckedLocations.Count > _reportedLocations.Count)
            {
                _archipelago.ReportCollectedLocations(allCheckedLocations.ToArray());
                AddRange(_reportedLocations, _newLocations);
                _newLocations.Clear();
            }

            CheckGoalCompletion();
        }

        private void TryToIdentifyUnknownLocationNames()
        {
            foreach (var locationName in _reportedLocations.Keys)
            {
                if (_reportedLocations[locationName] > -1)
                {
                    continue;
                }

                var locationId = _archipelago.GetLocationId(locationName);
                if (locationId == -1)
                {
                    continue;
                }

                _reportedLocations[locationName] = locationId;
            }
        }

        private void CheckGoalCompletion()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (!communityCenter.areAllAreasComplete())
            {
                return;
            }

            _archipelago.ReportGoalCompletion();
        }

        public void ReplaceAllLocationsRewardsWithChecks()
        {
            RemoveDefaultRewardsOnAllBundles();
            ReplaceCommunityCenterBundlesWithChecks();
            ReplaceCommunityCenterAreasWithChecks();
            ReplaceBackPackUpgradesWithChecks();
            ReplaceMineshaftChestsWithChecks();
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
                postfix: new HarmonyMethod(typeof(LocationsCodeInjection), nameof(LocationsCodeInjection.CheckForRewards_PostFix))
            );
        }

        private void ReplaceCommunityCenterAreasWithChecks()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            var areaCompleteRewardEventField = _modHelper.Reflection.GetField<NetEvent1Field<int, NetInt>>(communityCenter, "areaCompleteRewardEvent");

            areaCompleteRewardEventField.GetValue().Clear();
            areaCompleteRewardEventField.GetValue().onEvent += _locationsCodeInjection.DoAreaCompleteReward;
        }

        private void ReplaceBackPackUpgradesWithChecks()
        {
            if (_archipelago.SlotData.BackpackProgression == BackpackProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(LocationsCodeInjection), nameof(LocationsCodeInjection.PerformAction_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(LocationsCodeInjection), nameof(LocationsCodeInjection.AnswerDialogueAction_Prefix))
            );

            // This would need a transpile patch for SeedShop.draw and I don't think it's worth it.
            // _harmony.Patch(
            //     original: AccessTools.Method(typeof(SeedShop), nameof(SeedShop.draw)),
            //     transpiler: new HarmonyMethod(typeof(LocationsCodeInjection), nameof(LocationsCodeInjection.AnswerDialogueAction_Prefix)));
        }

        private void ReplaceMineshaftChestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(LocationsCodeInjection), nameof(LocationsCodeInjection.CheckForAction_Prefix))
            );
        }

        private static void AddRange<TKey, TValue>(Dictionary<TKey, TValue> srcDict, Dictionary<TKey, TValue> newDict)
        {
            foreach (var (key, value) in newDict)
            {
                srcDict.Add(key, value);
            }
        }
    }
}
