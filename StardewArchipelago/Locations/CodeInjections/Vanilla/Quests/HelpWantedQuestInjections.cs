using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewArchipelago.Locations.Festival;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using EventIds = StardewArchipelago.Constants.Vanilla.EventIds;
using Object = StardewValley.Object;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework.Content;
using StardewArchipelago.Archipelago;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Locations.Secrets;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Quests
{
    public static class HelpWantedQuestInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ContentManager _englishContentManager;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _englishContentManager = new ContentManager(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);
        }
        public static bool TryHandleQuestComplete(Quest quest, out bool runOriginal)
        {
            if (_archipelago.SlotData.QuestLocations.HelpWantedNumber <= 0)
            {
                runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                return true;
            }

            // Item Delivery: __instance.dailyQuest == true and questType == 3 [Chance: 40 / 65]
            // Copper Ores: Daily True, Type 1
            // Slay Monsters: Daily True, Type 4
            // Catch fish: Daily True, Type 7
            if (quest.dailyQuest.Value)
            {
                var isArchipelago = true;
                var numberOfSteps = GetNumberOfHelpWantedGroups();
                switch (quest.questType.Value)
                {
                    case (int)QuestType.ItemDelivery:
                        isArchipelago = CheckDailyQuestLocationOfType(DailyQuest.ITEM_DELIVERY, numberOfSteps * 4);
                        break;
                    case (int)QuestType.SlayMonsters:
                        isArchipelago = CheckDailyQuestLocationOfType(DailyQuest.SLAY_MONSTERS, numberOfSteps);
                        break;
                    case (int)QuestType.Fishing:
                        isArchipelago = CheckDailyQuestLocationOfType(DailyQuest.FISHING, numberOfSteps);
                        break;
                    case (int)QuestType.ResourceCollection:
                        isArchipelago = CheckDailyQuestLocationOfType(DailyQuest.GATHERING, numberOfSteps);
                        break;
                }

                if (!isArchipelago)
                {
                    runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                    return true;
                }

                ++Game1.stats.QuestsCompleted;
                QuestInjections.OriginalQuestCompleteCode(quest);
                runOriginal = MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                return true;
            }

            runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
            return false;
        }

        private static int GetNumberOfHelpWantedGroups()
        {
            var numberOfSteps = _archipelago.SlotData.QuestLocations.HelpWantedNumber / 7;
            if (_archipelago.SlotData.QuestLocations.HelpWantedNumber % 7 > 0)
            {
                numberOfSteps++;
            }

            return numberOfSteps;
        }

        private static bool CheckDailyQuestLocationOfType(string typeApName, int max)
        {
            var locationName = string.Format(DailyQuest.HELP_WANTED, typeApName);
            return CheckDailyQuestLocation(locationName, max);
        }

        public static bool CheckDailyQuestLocation(string locationName, int max)
        {
            if (GetNextDailyQuestLocation(locationName, max, out var nextQuestLocationName))
            {
                _locationChecker.AddCheckedLocation(nextQuestLocationName);
                return true;
            }

            return false;
        }

        private static bool GetNextDailyQuestLocation(string locationName, int max, out string nextLocationName)
        {
            nextLocationName = string.Empty;
            var nextLocationNumber = 1;
            while (nextLocationNumber <= max)
            {
                var fullName = $"{locationName} {nextLocationNumber}";
                var id = _archipelago.GetLocationId(fullName);
                if (id < 1)
                {
                    return false;
                }

                if (_locationChecker.IsLocationChecked(fullName))
                {
                    nextLocationNumber++;
                    continue;
                }

                nextLocationName = fullName;
                return true;
            }

            return false;
        }

        // public static Quest getQuestOfTheDay()
        public static bool GetQuestOfTheDay_BalanceQuests_Prefix(ref Quest __result)
        {
            try
            {
                if (Game1.stats.DaysPlayed <= 1U)
                {
                    __result = null;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var todayRandom = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
                var weightedLocations = CreateWeightedMissingLocations();
                if (!weightedLocations.Any())
                {
                    __result = null;
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var chosenIndex = todayRandom.Next(0, weightedLocations.Count);
                var chosenQuestType = weightedLocations[chosenIndex];
                switch (chosenQuestType)
                {
                    case DailyQuest.ITEM_DELIVERY:
                        __result = new ItemDeliveryQuest();
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case DailyQuest.FISHING:
                        __result = new FishingQuest();
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case DailyQuest.GATHERING:
                        __result = new ResourceCollectionQuest();
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    case DailyQuest.SLAY_MONSTERS:
                        __result = new SlayMonsterQuest();
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    default:
                        __result = null;
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetQuestOfTheDay_BalanceQuests_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static List<string> CreateWeightedMissingLocations()
        {
            var hints = _archipelago.GetMyActiveDesiredHints();
            var numberOfSteps = GetNumberOfHelpWantedGroups();
            var remainingHelpWantedQuests = new List<string>();
            for (var groupNumber = 1; groupNumber <= numberOfSteps; groupNumber++)
            {
                AddWeightedItemDeliveries(groupNumber, hints, remainingHelpWantedQuests);
                AddWeightedFishing(groupNumber, hints, remainingHelpWantedQuests);
                AddWeightedHelpWanted(groupNumber, DailyQuest.GATHERING, hints, remainingHelpWantedQuests);
                AddWeightedSlaying(groupNumber, hints, remainingHelpWantedQuests);
            }

            return remainingHelpWantedQuests;
        }

        private static void AddWeightedItemDeliveries(int groupNumber, Hint[] hints, List<string> remainingHelpWantedQuests)
        {
            const int itemDeliveryMultiplier = 4;
            var offset = ((groupNumber - 1) * itemDeliveryMultiplier) + 1;
            for (var delivery = 0; delivery < 4; delivery++)
            {
                AddWeightedHelpWanted(offset + delivery, DailyQuest.ITEM_DELIVERY, hints, remainingHelpWantedQuests);
            }
        }

        private static void AddWeightedFishing(int groupNumber, Hint[] hints, List<string> remainingHelpWantedQuests)
        {
            if (!_archipelago.HasReceivedItem(ToolUnlockManager.PROGRESSIVE_FISHING_ROD))
            {
                return;
            }

            AddWeightedHelpWanted(groupNumber, DailyQuest.FISHING, hints, remainingHelpWantedQuests);
        }

        private static void AddWeightedSlaying(int groupNumber, Hint[] hints, List<string> remainingHelpWantedQuests)
        {
            if (Game1.stats.MonstersKilled < 10)
            {
                return;
            }

            AddWeightedHelpWanted(groupNumber, DailyQuest.SLAY_MONSTERS, hints, remainingHelpWantedQuests);
        }

        private static void AddWeightedHelpWanted(int questNumber, string questType, Hint[] hints, List<string> remainingHelpWantedQuests)
        {
            var location = GetHelpWantedLocationName(questType, questNumber);
            if (!_locationChecker.IsLocationMissing(location))
            {
                return;
            }

            var weight = hints.Any(hint => _archipelago.GetLocationName(hint.LocationId) == location) ? 10 : 1;
            remainingHelpWantedQuests.AddRange(Enumerable.Repeat(questType, weight));
        }

        private static string GetHelpWantedLocationName(string type, int number)
        {
            return $"{string.Format(DailyQuest.HELP_WANTED, type)} {number}";
        }
    }
}
