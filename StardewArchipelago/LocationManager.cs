using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago
{
    public class LocationManager
    {
        private ArchipelagoClient _archipelago;
        private BundleReader _bundleReader;
        private IModHelper _modHelper;

        private List<long> _lastReportedLocations;
        private List<long> _newLocations;

        public LocationManager(ArchipelagoClient archipelago, BundleReader bundleReader, IModHelper modHelper)
        {
            _archipelago = archipelago;
            _bundleReader = bundleReader;
            _modHelper = modHelper;
            _lastReportedLocations = new List<long>();
            _newLocations = new List<long>();
        }

        public void CheckAllLocations(bool forceReport = false)
        {
            if (!_archipelago.IsConnected)
            {
                return;
            }

            var allCheckedLocations = new List<long>();
            allCheckedLocations.AddRange(CheckAllBundleLocations());

            allCheckedLocations.AddRange(_newLocations);
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
            areaCompleteRewardEventField.GetValue().onEvent += this.doAreaCompleteReward;
        }

        private void doAreaCompleteReward(int whichArea)
        {
            string AreaAPLocationName = "";
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
                    AreaAPLocationName = "Complete Bulletin";
                    break;
            }
            var completedAreaAPLocationId = _archipelago.GetLocationId(AreaAPLocationName);
            _newLocations.Add(completedAreaAPLocationId);
        }
    }
}
