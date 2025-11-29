using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Goals;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.Utilities.Interfaces;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer
{
    public static class MonsterSlayerInjections
    {
        public const string MONSTER_ERADICATION_AP_PREFIX = "Monster Eradication: ";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static MonsterKillList _killList;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, MonsterKillList killList)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _killList = killList;
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_NoMonsterSlayerRewards_Prefix(AdventureGuild __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tileIndex = __instance.getTileIndexAt(tileLocation, "Buildings", "1");
                _logger.LogInfo($"new Location({tileLocation.X}, {tileLocation.Y})");

                var gilLocations = new[]
                {
                    new Location(10, 12),
                    new Location(10, 13),
                    new Location(11, 11),
                    new Location(11, 12),
                    new Location(11, 13),
                    new Location(11, 13),
                    new Location(12, 12),
                    new Location(12, 13),
                };

                if (gilLocations.Any(x => x.Equals(tileLocation)))
                {
                    // private void gil()
                    var gilMethod = _modHelper.Reflection.GetMethod(__instance, "gil");
                    gilMethod.Invoke();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Gil_NoMonsterSlayerRewards_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }


        // private void gil()
        public static bool Gil_NoMonsterSlayerRewards_Prefix(AdventureGuild __instance)
        {
            try
            {
                var rewards = new List<Item>();
                var completedGoals = new List<KeyValuePair<string, MonsterSlayerQuestData>>();
                var values = new List<string>();
                foreach (var monsterSlayerQuest in DataLoader.MonsterSlayerQuests(Game1.content))
                {
                    var key = monsterSlayerQuest.Key;
                    var goal = monsterSlayerQuest.Value;
                    if (AdventureGuild.HasCollectedReward(Game1.player, key) || !AdventureGuild.IsComplete(goal))
                    {
                        continue;
                    }

                    completedGoals.Add(monsterSlayerQuest);
                    if (goal.RewardItemId != null)
                    {
                        var obj = ItemRegistry.Create(goal.RewardItemId);
                        obj.SpecialVariable = completedGoals.Count - 1;
                        if (obj is StardewValley.Object @object)
                        {
                            @object.specialItem = true;
                        }
                        if (obj is Hat hat)
                        {
                            rewards.Add(hat);
                        }
                    }
                    if (goal.RewardDialogue != null && (goal.RewardDialogueFlag == null || !Game1.player.mailReceived.Contains(goal.RewardDialogueFlag)))
                    {
                        values.Add(TokenParser.ParseText(goal.RewardDialogue));
                    }
                    if (goal.RewardMail != null)
                    {
                        Game1.addMailForTomorrow(goal.RewardMail);
                    }
                    if (goal.RewardMailAll != null)
                    {
                        Game1.addMailForTomorrow(goal.RewardMailAll, sendToEveryone: true);
                    }
                    if (goal.RewardFlag != null)
                    {
                        Game1.addMail(goal.RewardFlag, true);
                    }
                    if (goal.RewardFlagAll != null)
                    {
                        Game1.addMail(goal.RewardFlagAll, true, true);
                    }
                }
                if (rewards.Count > 0 || values.Count > 0)
                {
                    if (values.Count > 0)
                    {
                        Game1.DrawDialogue(new Dialogue(__instance.Gil, null, string.Join("#$b#", values)));
                        Game1.afterDialogues += () => OpenRewardMenuIfNeeded(__instance, rewards, completedGoals);
                    }
                    else
                    {
                        OpenRewardMenuIfNeeded(__instance, rewards, completedGoals);
                    }
                }
                else
                {
                    if (__instance.talkedToGil)
                    {
                        Game1.DrawDialogue(__instance.Gil, "Characters\\Dialogue\\Gil:Snoring");
                    }
                    else
                    {
                        Game1.DrawDialogue(__instance.Gil, "Characters\\Dialogue\\Gil:ComeBackLater");
                    }
                    __instance.talkedToGil = true;
                }
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Gil_NoMonsterSlayerRewards_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }
        private static void OpenRewardMenuIfNeeded(AdventureGuild adventureGuild, List<Item> rewards, List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals)
        {
            if (rewards.Count == 0)
            {
                return;
            }
            Game1.activeClickableMenu = new ItemGrabMenu(rewards, adventureGuild)
            {
                behaviorOnItemGrab = (item, who) => adventureGuild.OnRewardCollected(item, who, completedGoals)
            };
        }

        // public static bool areAllMonsterSlayerQuestsComplete()
        public static bool AreAllMonsterSlayerQuestsComplete_ExcludeGingerIsland_Prefix(ref bool __result)
        {
            try
            {
                __result = _killList.AreAllGoalsComplete();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AreAllMonsterSlayerQuestsComplete_ExcludeGingerIsland_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void showMonsterKillList()
        public static bool ShowMonsterKillList_CustomListFromAP_Prefix(AdventureGuild __instance)
        {
            try
            {
                if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
                {
                    Game1.player.mailReceived.Add("checkedMonsterBoard");
                }

                var killListContent = _killList.GetKillListLetterContent();
                Game1.drawLetterMessage(killListContent);

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShowMonsterKillList_CustomListFromAP_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void monsterKilled(string name)
        public static void MonsterKilled_SendMonstersanityCheck_Postfix(Stats __instance, string name)
        {
            try
            {
                var category = GetCategory(name);
                switch (_archipelago.SlotData.Monstersanity)
                {
                    case Monstersanity.None:
                        return;
                    case Monstersanity.OnePerCategory:
                        CheckLocation(category);
                        return;
                    case Monstersanity.OnePerMonster:
                        CheckLocation(name);
                        return;
                    case Monstersanity.Goals:
                    case Monstersanity.ShortGoals:
                    case Monstersanity.VeryShortGoals:
                        CheckLocationIfEnoughMonstersInCategory(category);
                        return;
                    case Monstersanity.ProgressiveGoals:
                        CheckLocationIfEnoughMonstersInProgressiveCategory(category);
                        return;
                    case Monstersanity.SplitGoals:
                        CheckLocationIfEnoughMonsters(name);
                        return;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MonsterKilled_SendMonstersanityCheck_Postfix)}:\n{ex}");
                return;
            }
        }

        // public void monsterKilled(string name)
        public static void MonsterKilled_CheckGoalCompletion_Postfix(Stats __instance, string name)
        {
            try
            {
                GoalCodeInjection.CheckProtectorOfTheValleyGoalCompletion();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MonsterKilled_CheckGoalCompletion_Postfix)}:\n{ex}");
                return;
            }
        }

        // public string Name { get; set; }
        public static void GetName_SkeletonMage_Postfix(Character __instance, ref string __result)
        {
            try
            {
                if (__instance is not Skeleton skeleton)
                {
                    return;
                }

                if (skeleton.isMage.Value)
                {
                    __result = MonsterName.SKELETON_MAGE;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetName_SkeletonMage_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void CheckLocationIfEnoughMonstersInCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || !_killList.MonsterGoals.ContainsKey(category))
            {
                return;
            }

            var amountNeeded = _killList.MonsterGoals[category];
            if (_killList.GetMonstersKilledInCategory(category) >= amountNeeded)
            {
                CheckLocation(category);
            }
        }

        private static void CheckLocationIfEnoughMonsters(string monster)
        {
            if (string.IsNullOrWhiteSpace(monster) || !_killList.MonsterGoals.ContainsKey(monster))
            {
                return;
            }

            var amountNeeded = _killList.MonsterGoals[monster];
            if (_killList.GetMonstersKilled(monster) >= amountNeeded)
            {
                CheckLocation(monster);
            }
        }

        private static void CheckLocationIfEnoughMonstersInProgressiveCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || !_killList.MonsterGoals.ContainsKey(category))
            {
                return;
            }

            var lastAmountNeeded = _killList.MonsterGoals[category];
            var progressiveStep = lastAmountNeeded / 5;
            var monstersKilled = _killList.GetMonstersKilledInCategory(category);
            for (var i = progressiveStep; i <= lastAmountNeeded; i += progressiveStep)
            {
                if (monstersKilled < i)
                {
                    return;
                }

                var progressiveCategoryName = (i == lastAmountNeeded) ? category : $"{i} {category}";
                CheckLocation(progressiveCategoryName);
            }
        }

        private static string GetCategory(string name)
        {
            foreach (var (category, monsters) in _killList.MonstersByCategory)
            {
                if (monsters.Contains(name))
                {
                    return category;
                }
            }

            _logger.LogDebug($"Could not find a monster slayer category for monster {name}");
            return "";
        }

        private static void CheckLocation(string goalName)
        {
            if (string.IsNullOrEmpty(goalName))
            {
                return;
            }

            goalName = goalName.Replace("Dust Spirit", "Dust Sprite");


            var apLocation = $"{MONSTER_ERADICATION_AP_PREFIX}{goalName}";
            if (_archipelago.GetLocationId(apLocation) > -1)
            {
                _locationChecker.AddCheckedLocation(apLocation);
            }
            else
            {
                _logger.LogDebug($"Tried to check a monster slayer goal, but it doesn't exist! [{apLocation}]");
            }
        }
    }
}
