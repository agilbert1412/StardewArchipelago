﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Microsoft.Xna.Framework.Content;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Unlocks.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.SpecialOrders;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Rewards;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Locations.Jojapocalypse.Consequences;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class SpecialOrderInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;
        private static ContentManager _englishContentManager;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _englishContentManager = new ContentManager(Game1.game1.Content.ServiceProvider, Game1.game1.Content.RootDirectory);
        }

        // public static bool IsSpecialOrdersBoardUnlocked()
        public static bool IsSpecialOrdersBoardUnlocked_UnlockBasedOnApItem_Prefix(ref bool __result)
        {
            try
            {
                __result = _archipelago.HasReceivedItem(VanillaUnlockManager.SPECIAL_ORDER_BOARD_AP_NAME);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(IsSpecialOrdersBoardUnlocked_UnlockBasedOnApItem_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;;
            }
        }

        // public static SpecialOrder GetSpecialOrder(string key, int? generation_seed)
        public static void GetSpecialOrder_ArchipelagoReward_Postfix(string key, int? generation_seed, ref SpecialOrder __result)
        {
            try
            {
                RemoveObsoleteRewards(__result);
                AdjustRequirements(__result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetSpecialOrder_ArchipelagoReward_Postfix)}:\n{ex}");
            }
        }

        private static void RemoveObsoleteRewards(SpecialOrder specialOrder)
        {
            var specialOrderName = GetEnglishQuestName(specialOrder.questName.Value);
            if (!_archipelago.LocationExists(specialOrderName))
            {
                return;
            }

            // Remove vanilla rewards if the player has not received the check.
            // We will keep vanilla rewards for repeated orders
            var checkMissing = _locationChecker.IsLocationMissing(specialOrderName);
            var shouldHaveVanillaRewards = IgnoredModdedStrings.SpecialOrders.Contains(specialOrderName);
            if (shouldHaveVanillaRewards)
            {
                return;
            }

            if (checkMissing)
            {
                specialOrder.rewards.Clear();
                Game1.player.team.specialOrders.Remove(specialOrder); // Might as well, and it cleans up SVE special orders.
                return;
            }

            // If the order has already been completed once, we can allow some non-unique rewards only
            for (var i = specialOrder.rewards.Count - 1; i >= 0; i--)
            {
                var reward = specialOrder.rewards[i];
                if (reward is MoneyReward or GemsReward or FriendshipReward)
                {
                    continue;
                }
                if (reward is ObjectReward objectReward)
                {
                    if (objectReward.itemKey.Value == "CalicoEgg")
                    {
                        continue;
                    }
                }
                specialOrder.rewards.RemoveAt(i);
            }
            return;
        }

        private static void AdjustRequirements(SpecialOrder specialOrder)
        {
            var requirementMultiplier = 1.0;
            if (_archipelago.SlotData.SpecialOrderLocations.HasFlag(SpecialOrderLocations.VeryShort))
            {
                requirementMultiplier = 0.2;
            }
            else if (_archipelago.SlotData.SpecialOrderLocations.HasFlag(SpecialOrderLocations.Short))
            {
                requirementMultiplier = 0.6;
            }

            requirementMultiplier = SpecialOrderConsequences.AdjustSpecialOrderAmountMultiplier(requirementMultiplier);

            if (Math.Abs(requirementMultiplier - 1.0) < 0.01)
            {
                return;
            }

            foreach (var objective in specialOrder.objectives)
            {
                if (objective.maxCount.Value <= 1)
                {
                    continue;
                }

                objective.maxCount.Value = Math.Max(1, (int)Math.Round(objective.maxCount.Value * requirementMultiplier));
            }
        }

        // public void CheckCompletion()
        public static void CheckCompletion_ArchipelagoReward_Postfix(SpecialOrder __instance)
        {
            try
            {
                if (__instance.questState.Value != SpecialOrderStatus.Complete)
                {
                    return;
                }

                var specialOrderName = GetEnglishQuestName(__instance.questName.Value);
                if (!_archipelago.LocationExists(specialOrderName))
                {
                    return;
                }

                _locationChecker.AddCheckedLocation(specialOrderName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckCompletion_ArchipelagoReward_Postfix)}:\n{ex}");
            }
        }

        // public virtual void SetDuration(QuestDuration duration)
        public static bool SetDuration_UseCorrectDateWithSeasonRandomizer_Prefix(SpecialOrder __instance, QuestDuration duration)
        {
            try
            {
                __instance.questDuration.Value = duration;
                var today = Game1.Date.TotalDays;
                switch (duration)
                {
                    case QuestDuration.Week:
                        // worldDate = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth - 1) / 7 * 7);
                        __instance.dueDate.Value = today + (7 - Game1.dayOfMonth % 7) + 1;
                        break;
                    case QuestDuration.Month:
                        __instance.dueDate.Value = today + (28 - Game1.dayOfMonth) + 1;
                        break;
                    case QuestDuration.TwoWeeks:
                        // worldDate = new WorldDate(Game1.year, Game1.currentSeason, (Game1.dayOfMonth - 1) / 7 * 7);
                        __instance.dueDate.Value = today + (14 - Game1.dayOfMonth % 7) + 1;
                        break;
                    case QuestDuration.OneDay:
                        __instance.dueDate.Value = today + 1;
                        break;
                    case QuestDuration.TwoDays:
                        __instance.dueDate.Value = today + 2;
                        break;
                    case QuestDuration.ThreeDays:
                        __instance.dueDate.Value = today + 3;
                        break;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetDuration_UseCorrectDateWithSeasonRandomizer_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;;
            }
        }

        // public static void UpdateAvailableSpecialOrders(string orderType, bool forceRefresh)
        public static bool UpdateAvailableSpecialOrders_ChangeFrequencyToBeLessRng_Prefix(string orderType, bool forceRefresh)
        {
            try
            {
                if (Game1.player.team.availableSpecialOrders­ is null)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                SetDurationOfSpecialOrders(Game1.player.team.availableSpecialOrders);

                // Let the game pick festival orders, they aren't checks anyway, right?
                if (orderType.Equals("DesertFestivalMarlon", StringComparison.InvariantCultureIgnoreCase) || (!_archipelago.SlotData.SpecialOrderLocations.HasFlag(SpecialOrderLocations.Board) && !_archipelago.SlotData.SpecialOrderLocations.HasFlag(SpecialOrderLocations.Qi)))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                UpdateAvailableSpecialOrdersBasedOnApState(orderType, forceRefresh);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(UpdateAvailableSpecialOrders_ChangeFrequencyToBeLessRng_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;;
            }
        }

        private static void UpdateAvailableSpecialOrdersBasedOnApState(string orderType, bool forceRefresh)
        {
            if (!forceRefresh)
            {
                if (Game1.player.team.availableSpecialOrders.Any(availableSpecialOrder => availableSpecialOrder.orderType.Value == orderType))
                {
                    return;
                }
            }

            SpecialOrder.RemoveAllSpecialOrders(orderType);

            var random = Utility.CreateRandom((double)Game1.uniqueIDForThisGame, (double)Game1.stats.DaysPlayed * 1.3);
            var allSpecialOrdersData = DataLoader.SpecialOrders(Game1.content);
            var specialOrdersThatCanBeStartedToday = FilterToSpecialOrdersThatCanBeStartedToday(allSpecialOrdersData, orderType);

            var specialOrderInstances = CreateSpecialOrderInstancesForType(specialOrdersThatCanBeStartedToday, orderType, random);

            var hints = _archipelago.GetMyActiveDesiredHints();

            ChooseTwoOrders(specialOrderInstances, hints, random);
        }

        private static void SetDurationOfSpecialOrders(IEnumerable<SpecialOrder> specialOrders)
        {
            foreach (var availableSpecialOrder in specialOrders)
            {
                if (availableSpecialOrder.questDuration.Value is QuestDuration.OneDay or QuestDuration.TwoDays or QuestDuration.ThreeDays &&
                    !Game1.player.team.acceptedSpecialOrderTypes.Contains(availableSpecialOrder.orderType.Value))
                {
                    availableSpecialOrder.SetDuration(availableSpecialOrder.questDuration.Value);
                }
            }
        }

        private static Dictionary<string, SpecialOrderData> FilterToSpecialOrdersThatCanBeStartedToday(
            Dictionary<string, SpecialOrderData> allSpecialOrdersData, string specialOrderType)
        {
            // A lot of this code is duplicated from SpecialOrder.CanStartOrderNow(orderId, order)
            // But I need to do something special with CheckTags so I had to split it and run it on my own

            var specialOrdersThatCanBeStartedToday = allSpecialOrdersData
                .Where(order => OrderTypeFilter(order, specialOrderType))
                .Where(RepeatableFilter)
                .Where(TooLateInMonthFilter)
                .Where(TagsFilter)
                .Where(ConditionsFilter)
                .Where(ActiveSpecialOrdersFilter)
                .Where(FishingRodFilter)
                .Where(ArcadeMachineFilter)
                .ToDictionary(x => x.Key, x => x.Value);
            return specialOrdersThatCanBeStartedToday;
        }

        private static bool OrderTypeFilter(KeyValuePair<string, SpecialOrderData> order, string specialOrderType)
        {
            return order.Value.OrderType == specialOrderType;
        }

        private static bool RepeatableFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return order.Value.Repeatable || !Game1.MasterPlayer.team.completedSpecialOrders.Contains(order.Key);
        }

        private static bool TooLateInMonthFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return Game1.dayOfMonth < 16 || order.Value.Duration != QuestDuration.Month;
        }

        private static bool TagsFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return CheckTags(order.Value.RequiredTags);
        }

        private static bool ConditionsFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return GameStateQuery.CheckConditions(order.Value.Condition);
        }

        private static bool ActiveSpecialOrdersFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return Game1.player.team.specialOrders.All(x => x.questKey.Value != order.Key);
        }

        private static bool FishingRodFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return !_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive) || !order.Key.StartsWith("Demetrius") || _archipelago.HasReceivedItem("Progressive Fishing Rod");
        }

        private static bool ArcadeMachineFilter(KeyValuePair<string, SpecialOrderData> order)
        {
            return _archipelago.SlotData.ArcadeMachineLocations != ArcadeLocations.Disabled || order.Key != SpecialOrders.LETS_PLAY_A_GAME;
        }

        private static bool CheckTags(string requiredTags)
        {
            if (requiredTags == null)
            {
                return true;
            }

            var splitTags = requiredTags.Split(",").Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            if (splitTags.Any(IsIslandTag))
            {
                var otherTags = splitTags.Where(x => !IsIslandTag(x));
                var otherTagsCondition = string.Join(',', otherTags);
                return HasUnlockedIslandAccess() && SpecialOrder.CheckTags(otherTagsCondition);
            }

            return SpecialOrder.CheckTags(requiredTags);
        }

        private static bool HasUnlockedIslandAccess()
        {
            return _archipelago.HasReceivedItem("Island Obelisk") || _archipelago.HasReceivedItem("Boat Repair");
        }

        private static bool IsIslandTag(string requiredTag)
        {
            return requiredTag.Equals("island", StringComparison.InvariantCultureIgnoreCase);
        }

        private static Dictionary<string, SpecialOrder> CreateSpecialOrderInstancesForType(
            Dictionary<string, SpecialOrderData> specialOrdersThatCanBeStartedToday, string orderType, Random random)
        {
            var specialOrders = specialOrdersThatCanBeStartedToday
                .Where(order => order.Value.OrderType == orderType)
                .Select(x => SpecialOrder.GetSpecialOrder(x.Key, random.Next()))
                .ToDictionary(x => x.GetName(), x => x);
            return specialOrders;
        }

        private static void ChooseTwoOrders(Dictionary<string, SpecialOrder> specialOrders,
            Hint[] hints, Random random)
        {
            const double chanceOfPreferentialPick = 0.75;

            var allSpecialOrders = specialOrders.Select(x => x.Key).ToList();

            var specialOrdersNeverCompletedBefore = allSpecialOrders.Where(key => StillNeedsToCompleteOrder(specialOrders, key)).ToList();

            var hintedSpecialOrders = specialOrdersNeverCompletedBefore.Where(key =>
                hints.Any(hint => _archipelago.GetLocationName(hint.LocationId) == specialOrders[key].GetName())).ToList();

            allSpecialOrders.RemoveAll(x => specialOrdersNeverCompletedBefore.Contains(x));
            specialOrdersNeverCompletedBefore.RemoveAll(x => hintedSpecialOrders.Contains(x));

            hintedSpecialOrders = hintedSpecialOrders.Shuffle(random);
            specialOrdersNeverCompletedBefore = specialOrdersNeverCompletedBefore.Shuffle(random);
            allSpecialOrders = allSpecialOrders.Shuffle(random);

            var allOrdersOrdered = new List<string>(hintedSpecialOrders);
            allOrdersOrdered.AddRange(specialOrdersNeverCompletedBefore);
            allOrdersOrdered.AddRange(allSpecialOrders);

            if (random.NextDouble() > chanceOfPreferentialPick)
            {
                allOrdersOrdered = allOrdersOrdered.Shuffle(random);
            }

            for (var i = 0; i < 2; ++i)
            {
                var order = allOrdersOrdered[i];
                Game1.player.team.availableSpecialOrders.Add(specialOrders[order]);
            }
        }

        private static bool StillNeedsToCompleteOrder(Dictionary<string, SpecialOrder> specialOrders, string key)
        {
            var orderName = specialOrders[key].GetName();
            if (_locationChecker.LocationExists(orderName) && _locationChecker.IsLocationMissing(orderName))
            {
                return true;
            }

            if (!Game1.player.team.completedSpecialOrders.Contains(key))
            {
                return true;
            }

            if (key == SpecialOrders.QIS_CROP && (_locationChecker.IsAnyLocationNotChecked("Qi Bean") || _locationChecker.IsAnyLocationNotChecked("Qi Crop")))
            {
                return true;
            }

            if (key == SpecialOrders.EXTENDED_FAMILY &&
                (_locationChecker.IsAnyLocationNotChecked("Son of Crimsonfish") ||
                 _locationChecker.IsAnyLocationNotChecked("Ms. Angler") ||
                 _locationChecker.IsAnyLocationNotChecked("Legend II") ||
                 _locationChecker.IsAnyLocationNotChecked("Glacierfish Jr.") ||
                 _locationChecker.IsAnyLocationNotChecked("Radioactive Carp")))
            {
                return true;
            }

            return false;
        }

        public static string GetEnglishQuestName(string questNameKey)
        {
            var specialOrderStrings = _englishContentManager.Load<Dictionary<string, string>>("Strings\\SpecialOrderStrings");
            questNameKey = questNameKey.Trim();
            int startIndex;
            do
            {
                startIndex = questNameKey.LastIndexOf('[');
                if (startIndex >= 0)
                {
                    var num = questNameKey.IndexOf(']', startIndex);
                    if (num == -1)
                    {
                        return questNameKey;
                    }

                    var str1 = questNameKey.Substring(startIndex + 1, num - startIndex - 1);
                    var thisString = specialOrderStrings.ContainsKey(str1) ? specialOrderStrings[str1] : SpecialOrderNames.Mods[str1];
                    questNameKey = questNameKey.Remove(startIndex, num - startIndex + 1);
                    questNameKey = questNameKey.Insert(startIndex, thisString);
                }
            } while (startIndex >= 0);

            return questNameKey;
        }
    }
}
