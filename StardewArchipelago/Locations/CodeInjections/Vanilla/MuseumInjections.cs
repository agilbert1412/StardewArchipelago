﻿using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Goals;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.CodeInjections;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class MuseumInjections
    {
        public const string MUSEUMSANITY_PREFIX = "Museumsanity:";
        private const string MUSEUMSANITY_TOTAL_DONATIONS = "{0} {1} Donations";
        private const string MUSEUMSANITY_TOTAL_MINERALS = "{0} {1} Minerals";
        private const string MUSEUMSANITY_TOTAL_ARTIFACTS = "{0} {1} Artifacts";
        private const string MUSEUMSANITY_ANCIENT_SEED = "Ancient Seed";
        public const string MUSEUMSANITY_DWARF_SCROLLS = "Dwarf Scrolls";
        public const string MUSEUMSANITY_SKELETON_FRONT = "Skeleton Front";
        public const string MUSEUMSANITY_SKELETON_MIDDLE = "Skeleton Middle";
        public const string MUSEUMSANITY_SKELETON_BACK = "Skeleton Back";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
        }

        // public List<Item> getRewardsForPlayer(Farmer player)
        public static bool GetRewardsForPlayer_Museumsanity_Prefix(LibraryMuseum __instance, Farmer player, ref List<Item> __result)
        {
            try
            {
                var rewards = new List<Item>();
                var museumItems = new HashSet<string>(__instance.museumPieces.Values);
                var museumRewardData = DataLoader.MuseumRewards(Game1.content);
                var donatedItemsByTag = __instance.GetDonatedByContextTag(museumRewardData);
                var numberOfArtifactsDonated = donatedItemsByTag["item_type_arch"];
                var numberOfMineralsDonated = donatedItemsByTag["item_type_minerals"];
                var totalNumberDonated = donatedItemsByTag[""];

                SendSpecialMuseumLetters(player, totalNumberDonated);
                CheckMilestones(numberOfArtifactsDonated, numberOfMineralsDonated, totalNumberDonated);
                CheckSpecialCollections(museumItems);
                CheckSpecificItems(museumItems);

                if (totalNumberDonated > 0)
                {
                    _locationChecker.AddCheckedLocation(QuestLogInjections.ARCHAEOLOGY_QUEST_NAME);
                }

                __result = rewards;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRewardsForPlayer_Museumsanity_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public List<Item> getRewardsForPlayer(Farmer player)
        public static void GetRewardsForPlayer_CheckGoalCompletion_Postfix(LibraryMuseum __instance, Farmer player, ref List<Item> __result)
        {
            try
            {
                GoalCodeInjection.CheckCompleteCollectionGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRewardsForPlayer_CheckGoalCompletion_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void SendSpecialMuseumLetters(Farmer who, int totalNumberDonated)
        {
            SendLetterForTotal(who, "museum5", totalNumberDonated, 5);
            SendLetterForTotal(who, "museum10", totalNumberDonated, 10);
            SendLetterForTotal(who, "museum15", totalNumberDonated, 15);
            SendLetterForTotal(who, "museum20", totalNumberDonated, 20);
            SendLetterForTotal(who, "museum25", totalNumberDonated, 25);
            SendLetterForTotal(who, "museum30", totalNumberDonated, 30);
            SendLetterForTotal(who, "museum35", totalNumberDonated, 35);
            SendLetterForTotal(who, "museum40", totalNumberDonated, 40);
            SendLetterForTotal(who, "museum50", totalNumberDonated, 50);
            if (totalNumberDonated >= 60)
            {
                Game1.player.eventsSeen.Add("295672");
                Game1.player.eventsSeen.Add("66");
            }

            SendLetterForTotal(who, "museum70", totalNumberDonated, 70);
            SendLetterForTotal(who, "museum80", totalNumberDonated, 80);
            SendLetterForTotal(who, "museum90", totalNumberDonated, 90);
            SendLetterForTotal(who, "museumComplete", totalNumberDonated, 95);
        }

        private static void SendLetterForTotal(Farmer who, string letter, int totalNumberDonated, int threshold)
        {
            if (!who.mailReceived.Contains(letter) && totalNumberDonated >= threshold)
            {
                who.mailReceived.Add(letter);
            }
        }

        private static void CheckMilestones(int numberOfArtifactsDonated, int numberOfMineralsDonated, int totalNumberDonated)
        {
            if (_archipelago.SlotData.Museumsanity != Museumsanity.Milestones)
            {
                return;
            }

            CheckMilestonesForType(MUSEUMSANITY_TOTAL_ARTIFACTS, numberOfArtifactsDonated);
            CheckMilestonesForType(MUSEUMSANITY_TOTAL_MINERALS, numberOfMineralsDonated);
            CheckMilestonesForType(MUSEUMSANITY_TOTAL_DONATIONS, totalNumberDonated);
        }

        private static void CheckMilestonesForType(string apLocationTemplate, int numberDonated)
        {
            for (var i = 1; i <= numberDonated; i++)
            {
                var apLocation = string.Format(apLocationTemplate, MUSEUMSANITY_PREFIX, i);
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
            }
        }

        private static void CheckSpecialCollections(HashSet<string> museumItems)
        {
            if (_archipelago.SlotData.Museumsanity != Museumsanity.Milestones)
            {
                return;
            }

            CheckSpecialCollection(museumItems, new[] { "96", "97", "98", "99" }, MUSEUMSANITY_DWARF_SCROLLS);
            CheckSpecialCollection(museumItems, new[] { "114" }, MUSEUMSANITY_ANCIENT_SEED);
            CheckSpecialCollection(museumItems, new[] { "579", "581", "582" }, MUSEUMSANITY_SKELETON_FRONT);
            CheckSpecialCollection(museumItems, new[] { "583", "584" }, MUSEUMSANITY_SKELETON_MIDDLE);
            CheckSpecialCollection(museumItems, new[] { "580", "585" }, MUSEUMSANITY_SKELETON_BACK);
        }

        private static void CheckSpecialCollection(HashSet<string> museumItems, string[] requiredItems, string apSubLocation)
        {
            if (requiredItems.All(museumItems.Contains))
            {
                var apLocation = $"{MUSEUMSANITY_PREFIX} {apSubLocation}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
            }
        }

        private static void CheckSpecificItems(HashSet<string> museumItems)
        {
            if (_archipelago.SlotData.Museumsanity == Museumsanity.Milestones)
            {
                return;
            }

            foreach (var museumItemId in museumItems)
            {
                var donatedItem = _itemManager.GetObjectById(museumItemId);
                var donatedItemName = donatedItem.Name;
                var apLocation = $"{MUSEUMSANITY_PREFIX} {donatedItemName}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
                else
                {
                    _logger.LogError($"Unrecognized Museumsanity Location: {donatedItemName} [{museumItemId}]");
                }
            }
        }
    }
}
