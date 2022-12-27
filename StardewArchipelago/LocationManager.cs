using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago
{
    public class LocationManager
    {
        private static IMonitor _monitor;
        private ArchipelagoClient _archipelago;
        private BundleReader _bundleReader;
        private IModHelper _modHelper;
        private Harmony _harmony;
        private LocationsCodeInjection _locationsCodeInjection;

        private List<long> _lastReportedLocations;
        private List<long> _newLocations;

        public LocationManager(IMonitor monitor, ArchipelagoClient archipelago, BundleReader bundleReader, IModHelper modHelper, Harmony harmony)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _bundleReader = bundleReader;
            _modHelper = modHelper;
            _harmony = harmony;
            _lastReportedLocations = new List<long>();
            _newLocations = new List<long>();
            _locationsCodeInjection = new LocationsCodeInjection(_monitor, _archipelago, (id) => _newLocations.Add(id));
        }

        public void CheckAllLocations(bool forceReport = false)
        {
            if (!_archipelago.IsConnected)
            {
                return;
            }

            var allCheckedLocations = new List<long>();
            allCheckedLocations.AddRange(_lastReportedLocations);
            allCheckedLocations.AddRange(CheckAllBundleLocations());
            allCheckedLocations.AddRange(_newLocations);

            allCheckedLocations = allCheckedLocations.Distinct().ToList();
            if (forceReport || allCheckedLocations.Count > _lastReportedLocations.Count || _newLocations.Any())
            {
                _archipelago.ReportCollectedLocations(allCheckedLocations.ToArray());
                _lastReportedLocations = allCheckedLocations;
                _newLocations.Clear();
            }

            CheckGoalCompletion();
        }

        private IEnumerable<long> CheckAllBundleLocations()
        {
            var bundleStates = _bundleReader.ReadCurrentBundleStates();
            var completedBundleNames = bundleStates.Where(x => x.IsCompleted).Select(x => x.RelatedBundle.BundleName);
            var completedBundleAPIds = completedBundleNames.Select(x => _archipelago.GetLocationId(x + " Bundle"));
            return completedBundleAPIds;
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

        public void RemoveDefaultRewardsOnAllLocations()
        {
            RemoveDefaultRewardsOnAllBundles();
            RemoveDefaultRewardsOnCommunityCenterAreas();
            RemoveBackPackIncreaseOnBackpackPurchase();
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

        private void RemoveDefaultRewardsOnCommunityCenterAreas()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            var areaCompleteRewardEventField = _modHelper.Reflection.GetField<NetEvent1Field<int, NetInt>>(communityCenter, "areaCompleteRewardEvent");

            areaCompleteRewardEventField.GetValue().Clear();
            areaCompleteRewardEventField.GetValue().onEvent += _locationsCodeInjection.DoAreaCompleteReward;
        }

        private void RemoveBackPackIncreaseOnBackpackPurchase()
        {
            var seedShop = Game1.locations.OfType<SeedShop>().First();
            var answerDialogMethod = _modHelper.Reflection.GetMethod(seedShop, "answerDialogueAction");

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
    }
}
