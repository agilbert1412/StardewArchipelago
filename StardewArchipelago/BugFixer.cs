using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago
{
    public class BugFixer
    {
        private readonly StardewArchipelagoClient _archipelago;
        private readonly ILogger _logger;
        private readonly LocationChecker _locationChecker;

        public BugFixer(StardewArchipelagoClient archipelago, ILogger logger, LocationChecker locationChecker)
        {
            _archipelago = archipelago;
            _logger = logger;
            _locationChecker = locationChecker;
        }


        public void FixKnownBugs()
        {
            FixFreeBuildingsWithFreeInTheId();
            FixBundleRoomsNotProperlyCompleted();
            TryFixStardrops();
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

        private void TryFixStardrops()
        {
            var allUpcomingMail = Game1.player.mailForTomorrow.Union(Game1.player.mailbox);
            if (allUpcomingMail.Any(x => x.Contains("AP|Stardrop|")))
            {
                return;
            }

            FixStardrops();
        }

        private void FixStardrops()
        {
            var numberStardropsDeserved = _archipelago.GetReceivedItemCount(APItem.STARDROP);

            if (_archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla && Game1.player.mailReceived.Contains("CF_Fair"))
            {
                numberStardropsDeserved++;
            }

            if (_archipelago.SlotData.Friendsanity == Friendsanity.None && Game1.player.mailReceived.Contains("CF_Spouse"))
            {
                numberStardropsDeserved++;
            }

            if (!_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.Easy) && Game1.player.mailReceived.Contains("CF_Statue"))
            {
                numberStardropsDeserved++;
            }

            if (_archipelago.SlotData.Fishsanity == Fishsanity.None && Game1.player.mailReceived.Contains("CF_Fish"))
            {
                numberStardropsDeserved++;
            }

            var expectedMaxStamina = 270 + (numberStardropsDeserved * 34);

            if (Game1.player.MaxStamina < expectedMaxStamina)
            {
                _logger.LogWarning($"Detected the player max stamina is lower than expected, based on found Stardrops. Current Max: {Game1.player.MaxStamina}, Expected: {expectedMaxStamina}. StardewArchipelago will attempt to automatically fix the problem.");
                Game1.player.maxStamina.Value = expectedMaxStamina;
            }
        }
    }
}
