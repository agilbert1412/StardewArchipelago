using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Netcode;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace StardewArchipelago
{
    public class BugFixer
    {
        private readonly ILogger _logger;
        private readonly LocationChecker _locationChecker;

        public BugFixer(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }


        public void FixKnownBugs()
        {
            FixFreeBuildingsWithFreeInTheId();
            FixBundleRoomsNotProperlyCompleted();
        }

        private static void FixFreeBuildingsWithFreeInTheId()
        {

            foreach (var gameLocation in Game1.locations)
            {
                foreach (var building in gameLocation.buildings)
                {
                    building.buildingType.Set(CarpenterInjections.RemoveFreePrefix(building.buildingType.Get()));
                }
            }
        }

        private void FixBundleRoomsNotProperlyCompleted()
        {
            var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
            for (var area = 0; area < communityCenter.areasComplete.Count; area++)
            {
                var areaComplete = communityCenter.areasComplete[area];
                var isBundleRemaining = IsThereAnIncompleteBundleInArea(communityCenter, area);
                if (areaComplete && isBundleRemaining)
                {
                    communityCenter.areasComplete[area] = false;
                }
                else if(!areaComplete && !isBundleRemaining)
                {
                    communityCenter.areasComplete[area] = true;
                }
            }
        }

        public bool IsThereAnIncompleteBundleInArea(CommunityCenter communityCenter, int area)
        {
            var areaToBundleDictionary = GetAreaToBundleDictionary(communityCenter);
            foreach (var bundleId in areaToBundleDictionary[area])
            {
                if (!communityCenter.bundles.TryGetValue(bundleId, out var bundleState))
                {
                    continue;
                }
                var numberIngredientsToFill = bundleState.Length / 3;
                for (var ingredientIndex = 0; ingredientIndex < numberIngredientsToFill; ++ingredientIndex)
                {
                    if (!bundleState[ingredientIndex])
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Dictionary<int, List<int>> GetAreaToBundleDictionary(CommunityCenter communityCenter)
        {
            var areaToBundleDictionary = new Dictionary<int, List<int>>();
            for (var key = 0; key < 7; ++key)
            {
                areaToBundleDictionary.Add(key, new List<int>());
                var netMutex = new NetMutex();
                communityCenter.bundleMutexes.Add(netMutex);
                communityCenter.NetFields.AddField(netMutex.NetFields, "m.NetFields");
            }

            foreach (var keyValuePair in Game1.netWorldState.Value.BundleData)
            {
                var int32 = Convert.ToInt32(keyValuePair.Key.Split('/')[1]);
                areaToBundleDictionary[CommunityCenter.getAreaNumberFromName(keyValuePair.Key.Split('/')[0])].Add(int32);
                // this.bundleToAreaDictionary.Add(int32, CommunityCenter.getAreaNumberFromName(keyValuePair.Key.Split('/')[0]));
            }

            return areaToBundleDictionary;
        }
    }
}
