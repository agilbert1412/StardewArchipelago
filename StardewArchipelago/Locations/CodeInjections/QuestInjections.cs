using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.ObjectModel;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.Locations.CodeInjections
{
    public static class QuestInjections
    {
        private static readonly string[] _ignoredQuests = {
            "To The Beach", "Explore The Mine", "Deeper In The Mine", "To The Bottom?", "The Mysterious Qi",
            "A Winter Mystery", "Cryptic Note", "Dark Talisman", "Goblin Problem", "The Pirate's Wife"
        };

        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public static bool QuestComplete_LocationInsteadOfReward_Prefix(Quest __instance)
        {
            try
            {
                var questName = __instance.GetName();
                if (__instance.completed.Value || _ignoredQuests.Contains(questName))
                {
                    return true; // run original logic
                }

                // Item Delivery: __instance.dailyQuest == true and questType == 3 [Chance: 40 / 65]
                // Copper Ores: Daily True, Type 10 [Chance: 8 / 65]
                // Slay Monsters: Daily True, Type 4 [Chance: 10 / 65]
                // Catch fish: Daily Trye, Type 7 [Chance: 7 / 65]
                if (__instance.dailyQuest.Value)
                {
                    var isArchipelago = true;
                    switch (__instance.questType.Value)
                    {
                        case (int)QuestType.ItemDelivery:
                            isArchipelago = CheckDailyQuestLocationOfType("Item Delivery", _archipelago.SlotData.HelpWantedLocationNumber * 4 / 7);
                            break;
                        case (int)QuestType.SlayMonsters:
                            isArchipelago = CheckDailyQuestLocationOfType("Slay Monsters", _archipelago.SlotData.HelpWantedLocationNumber / 7);
                            break;
                        case (int)QuestType.Fishing:
                            isArchipelago = CheckDailyQuestLocationOfType("Fishing", _archipelago.SlotData.HelpWantedLocationNumber / 7);
                            break;
                        case (int)QuestType.ResourceCollection:
                            isArchipelago = CheckDailyQuestLocationOfType("Gathering", _archipelago.SlotData.HelpWantedLocationNumber / 7);
                            break;
                    }

                    if (!isArchipelago)
                    {
                        return true; // run original logic
                    }

                    ++Game1.stats.QuestsCompleted;
                }
                else
                {
                    // Story Quest
                    _locationChecker.AddCheckedLocation(questName);
                }

                OriginalQuestCompleteCode(__instance);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(QuestComplete_LocationInsteadOfReward_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void OriginalQuestCompleteCode(Quest __instance)
        {
            __instance.completed.Value = true;
            if (__instance.nextQuests.Count > 0)
            {
                foreach (var nextQuest in __instance.nextQuests.Where(nextQuest => nextQuest > 0))
                {
                    Game1.player.questLog.Add(Quest.getQuestFromId(nextQuest));
                }

                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Quest.cs.13636"), 2));
            }

            Game1.player.questLog.Remove(__instance);
            Game1.playSound("questcomplete");
            if (__instance.id.Value == 126)
            {
                Game1.player.mailReceived.Add("emilyFiber");
                Game1.player.activeDialogueEvents.Add("emilyFiber", 2);
            }

            Game1.dayTimeMoneyBox.questsDirty = true;
        }

        private static bool CheckDailyQuestLocationOfType(string typeApName, int max)
        {
            var locationName = $"Help Wanted: {typeApName}";
            return CheckDailyQuestLocation(locationName, max);
        }

        public static bool CheckDailyQuestLocation(string locationName, int max)
        {
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

                _locationChecker.AddCheckedLocation(fullName);
                return true;
            }

            return false;
        }

        public static void Command_RemoveQuest_CheckLocation_Postfix(Event __instance, GameLocation location, GameTime time, string[] split)
        {
            try
            {
                var questId = Convert.ToInt32(split[1]);
                var quest = Quest.getQuestFromId(questId);
                var questName = quest.GetName();
                if (_ignoredQuests.Contains(questName))
                {
                    return;
                }
                _locationChecker.AddCheckedLocation(questName);

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Command_RemoveQuest_CheckLocation_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static bool CheckAction_AdventurerGuild_Prefix(Mountain __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                var tile = __instance.map.GetLayer("Buildings").Tiles[tileLocation];
                if (tile == null || tile.TileIndex != 1136)
                {
                    return true; // run original logic
                }

                if (!who.mailReceived.Contains("guildMember"))
                {
                    Game1.drawLetterMessage(Game1.content.LoadString("Strings\\Locations:Mountain_AdventurersGuildNote").Replace('\n', '^'));
                    __result = true;
                    return false; // don't run original logic
                }

                string action = null;
                //var tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                PropertyValue propertyValue;
                tile.Properties.TryGetValue("Action", out propertyValue);
                if (propertyValue != null)
                    action = propertyValue.ToString();
                if (action == null)
                {
                    action = __instance.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
                }
                if (action != null)
                {
                    __result = __instance.performAction(action, who, tileLocation);
                }

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_AdventurerGuild_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool PerformAction_MysteriousQiLumberPile_Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true; // run original logic
                }

                var actionWords = action.Split(' ');
                var actionFirstWord = actionWords[0];

                if (actionFirstWord != "LumberPile" || who.hasOrWillReceiveMail("TH_LumberPile") || !who.hasOrWillReceiveMail("TH_SandDragon"))
                {
                    return true; // run original logic
                }

                Game1.player.mailReceived.Add("TH_LumberPile");
                Game1.player.removeQuest(5);
                _locationChecker.AddCheckedLocation("The Mysterious Qi");

                __result = true;
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_MysteriousQiLumberPile_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
        
        public static bool Shake_WinterMysteryBush_Prefix(Bush __instance, Vector2 tileLocation,
            bool doEvenIfStillShaking)
        {
            try
            {
                var maxShakeField = _helper.Reflection.GetField<float>(__instance, "maxShake");
                if (!((double)maxShakeField.GetValue() == 0.0 || doEvenIfStillShaking))
                {
                    return true; // run original logic
                }

                var correctLocation = (double)tileLocation.X == 28.0 && (double)tileLocation.Y == 14.0;
                var hasSeenKrobusEvent = Game1.player.eventsSeen.Contains(520702);
                var hasNotCompletedQuest = _locationChecker.IsLocationMissing("A Winter Mystery");
                var isInTown = Game1.currentLocation is Town town;

                if (!correctLocation || !hasSeenKrobusEvent || (!hasNotCompletedQuest && !Game1.player.hasQuest(31)) || !isInTown)
                {
                    return true; // run original logic
                }

                ((Town)(Game1.currentLocation)).initiateMagnifyingGlassGet();
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Shake_WinterMysteryBush_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool MgThief_AfterSpeech_WinterMysteryFinished_Prefix(Town __instance)
        {
            try
            {
                var afterGlassMethod = _helper.Reflection.GetMethod(__instance, "mgThief_afterGlass");
                Game1.afterDialogues = () => afterGlassMethod.Invoke();
                Game1.player.removeQuest(31);
                _locationChecker.AddCheckedLocation("A Winter Mystery");
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(MgThief_AfterSpeech_WinterMysteryFinished_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void SkillsPageCtor_BearKnowledge_Postfix(SkillsPage __instance, int x, int y, int width, int height)
        {
            try
            {
                const int bearKnowledgeIndex = 8;
                int x1 = __instance.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 80;
                int y1 = __instance.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + (int)(height / 2.0) + 80;
                if (_archipelago.HasReceivedItem("Bear's Knowledge", out _))
                {
                    ClickableTextureComponent textureComponent = new ClickableTextureComponent("", new Rectangle(x1 + 544, y1, 64, 64), null, Game1.content.LoadString("Strings\\Objects:BearPaw"), Game1.mouseCursors, new Rectangle(192, 336, 16, 16), 4f, true);
                    textureComponent.myID = 10208;
                    textureComponent.rightNeighborID = -99998;
                    textureComponent.leftNeighborID = -99998;
                    textureComponent.upNeighborID = 4;
                    __instance.specialItems[bearKnowledgeIndex] = textureComponent;
                }
                else
                {
                    __instance.specialItems[bearKnowledgeIndex] = null;
                }


                int num1 = 680 / __instance.specialItems.Count;
                for (int index = 0; index < __instance.specialItems.Count; ++index)
                {
                    if (__instance.specialItems[index] != null)
                        __instance.specialItems[index].bounds.X = x1 + index * num1;
                }
                ClickableComponent.SetUpNeighbors<ClickableTextureComponent>(__instance.specialItems, 4);
                ClickableComponent.ChainNeighborsLeftRight<ClickableTextureComponent>(__instance.specialItems);

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SkillsPageCtor_BearKnowledge_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void GetPriceAfterMultipliers_BearKnowledge_Postfix(Object __instance, float startPrice, long specificPlayerID, ref float __result)
        {
            try
            {
                if (__instance.ParentSheetIndex != 296 && __instance.ParentSheetIndex != 410)
                {
                    return;
                }

                var hasSeenBearEvent = Game1.player.eventsSeen.Contains(2120303);
                var hasReceivedBearKnowledge = _archipelago.HasReceivedItem("Bear's Knowledge", out _);
                if (hasSeenBearEvent == hasReceivedBearKnowledge)
                {
                    return;
                }

                if (hasReceivedBearKnowledge)
                {
                    __result *= 3f;
                    return;
                }

                __result /= 3f;
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetPriceAfterMultipliers_BearKnowledge_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static bool Command_AwardFestivalPrize_QiMilk_Prefix(Event __instance, GameLocation location,
            GameTime time, string[] split)
        {
            try
            {
                if (split.Length < 2)
                {
                    return true; // run original logic
                }
                string lower = split[1].ToLower();
                if (lower != "qimilk")
                {
                    return true; // run original logic
                }

                if (!Game1.player.mailReceived.Contains("qiCave"))
                {
                    _locationChecker.AddCheckedLocation("Cryptic Note");
                    Game1.player.mailReceived.Add("qiCave");

                    if (_archipelago.SlotData.Goal == Goal.CrypticNote)
                    {
                        _archipelago.ReportGoalCompletion();
                    }
                }
                ++__instance.CurrentCommand;

                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Command_AwardFestivalPrize_QiMilk_Prefix)}:\n{ex}",
                    LogLevel.Error);
                return true; // run original logic
            }
        }
    }

    public enum QuestType
    {
        Basic = 1,
        Crafting = 2,
        ItemDelivery = 3,
        Monster = 4,
        SlayMonsters = 4,
        Socialize = 5,
        Location = 6,
        Fishing = 7,
        Building = 8,
        ItemHarvest = 9,
        LostItem = 9,
        SecretLostItem = 9,
        ResourceCollection = 10,
    }
}
