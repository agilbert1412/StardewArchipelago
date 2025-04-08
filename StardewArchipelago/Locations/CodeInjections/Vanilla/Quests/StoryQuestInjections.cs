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
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Locations.Secrets;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Quests
{
    public static class StoryQuestInjections
    {
        private static readonly List<string> _ignoredQuests = new()
        {
            "To The Beach", "Explore The Mine", "Deeper In The Mine", "To The Bottom?", "The Mysterious Qi",
            "A Winter Mystery", "Cryptic Note", "Dark Talisman", "Goblin Problem",
        };

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
            UpdateIgnoredQuestList();
        }

        public static bool TryHandleQuestComplete(Quest quest, out bool runOriginal)
        {
            var questName = quest.GetName();
            var englishQuestName = GetQuestEnglishName(quest.id.Value, questName);
            if (_ignoredQuests.Contains(englishQuestName))
            {
                runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                return true;
            }

            if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
            {
                runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
                return true;
            }

            if (!quest.dailyQuest.Value)
            {
                _locationChecker.AddCheckedLocation(englishQuestName);
                QuestInjections.OriginalQuestCompleteCode(quest);
                runOriginal = MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                return true;
            }

            runOriginal = MethodPrefix.RUN_ORIGINAL_METHOD;
            return false;
        }

        private static string GetQuestEnglishName(string questId, string defaultName)
        {
            var englishQuests = _englishContentManager.Load<Dictionary<string, string>>("Data\\Quests");

            if (string.IsNullOrWhiteSpace(questId) || !englishQuests.ContainsKey(questId))
            {
                return defaultName;
            }

            var equivalentEnglishQuestString = englishQuests[questId];
            var englishTitle = equivalentEnglishQuestString.Split('/')[1];
            return englishTitle;
        }

        // public static void RemoveQuest(Event @event, string[] args, EventContext context)
        public static void RemoveQuest_CheckLocation_Postfix(Event @event, string[] args, EventContext context)
        {
            try
            {
                var questId = args[1];
                var quest = Quest.getQuestFromId(questId);
                var questName = quest.GetName();
                var englishQuestName = GetQuestEnglishName(questId, questName);
                if (_ignoredQuests.Contains(englishQuestName))
                {
                    return;
                }
                _locationChecker.AddCheckedLocation(englishQuestName);
                SecretNotesInjections.TryHandleQuestComplete(quest, out _);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(RemoveQuest_CheckLocation_Postfix)}:\n{ex}");
                return;
            }
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_MysteriousQiLumberPile_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled && !_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.SecretNotes))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (action == null || !who.IsLocalPlayer)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGet(action, 0, out var actionName, out _, name: "string actionType"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (actionName != "LumberPile" || who.hasOrWillReceiveMail("TH_LumberPile") || !who.hasOrWillReceiveMail("TH_SandDragon"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                Game1.player.mailReceived.Add("TH_LumberPile");
                Game1.player.removeQuest("5");
                _locationChecker.AddCheckedLocations(new [] {"The Mysterious Qi", SecretsLocationNames.SECRET_NOTE_22});

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_MysteriousQiLumberPile_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        public static bool Shake_WinterMysteryBush_Prefix(Bush __instance, Vector2 tileLocation,
            bool doEvenIfStillShaking)
        {
            try
            {
                var maxShakeField = _helper.Reflection.GetField<float>(__instance, "maxShake");
                if (!(maxShakeField.GetValue() == 0.0 || doEvenIfStillShaking))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var shakeLeftField = _helper.Reflection.GetField<bool>(__instance, "shakeLeft");

                var playerIsOnTheRight = Game1.player.Tile.X > tileLocation.X;
                var playerIsAlignedVertically = Game1.player.Tile.X == tileLocation.X;

                shakeLeftField.SetValue(playerIsOnTheRight || playerIsAlignedVertically && Game1.random.NextDouble() < 0.5);
                maxShakeField.SetValue((float)Math.PI / 128f);
                var isTownBush = __instance.townBush.Value;
                var isInBloom = __instance.inBloom();
                if (!isTownBush && __instance.tileSheetOffset.Value == 1 && isInBloom)
                {
                    ShakeForBushItem(__instance, tileLocation);
                    return false; // run original logic;
                }

                if (tileLocation.X == 20.0 && tileLocation.Y == 8.0 && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200 && !Game1.player.mailReceived.Contains("junimoPlush"))
                {
                    GetJunimoPlush(__instance);
                }
                else if (tileLocation.X == 28.0 && tileLocation.Y == 14.0 && Game1.player.eventsSeen.Contains("520702") && Game1.player.hasQuest("31") && Game1.currentLocation is Town currentTown)
                {
                    currentTown.initiateMagnifyingGlassGet();
                }
                else
                {
                    if (tileLocation.X != 47.0 || tileLocation.Y != 100.0 ||
                        !Game1.player.secretNotesSeen.Contains(21) || Game1.timeOfDay != 2440 ||
                        !(Game1.currentLocation is Town) || Game1.player.mailReceived.Contains("secretNote21_done"))
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    Game1.player.mailReceived.Add("secretNote21_done");
                    ((Town)Game1.currentLocation).initiateMarnieLewisBush();
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Shake_WinterMysteryBush_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ShakeForBushItem(Bush bush, Vector2 tileLocation)
        {

            var shakeOffItem = bush.GetShakeOffItem();
            if (string.IsNullOrWhiteSpace(shakeOffItem))
            {
                return;
            }

            bush.tileSheetOffset.Value = 0;
            bush.setUpSourceRect();
            switch (bush.size.Value)
            {
                case 3:
                    Game1.createObjectDebris(shakeOffItem, (int)tileLocation.X, (int)tileLocation.Y);
                    break;
                case 4:
                    bush.uniqueSpawnMutex.RequestLock((Action)(() =>
                    {
                        Game1.player.team.MarkCollectedNut($"Bush_{bush.Location.Name}_{tileLocation.X}_{tileLocation.Y}");
                        var obj = ItemRegistry.Create(shakeOffItem);
                        var boundingBox = bush.getBoundingBox();
                        var x = (double)boundingBox.Center.X;
                        boundingBox = bush.getBoundingBox();
                        var y = (double)(boundingBox.Bottom - 2);
                        var pixelOrigin = new Vector2((float)x, (float)y);
                        var location = bush.Location;
                        boundingBox = bush.getBoundingBox();
                        var bottom = boundingBox.Bottom;
                        Game1.createItemDebris(obj, pixelOrigin, 0, location, bottom);
                    }));
                    break;
                default:
                    var random = Utility.CreateRandom((double)tileLocation.X, (double)tileLocation.Y * 5000.0, (double)Game1.uniqueIDForThisGame, (double)Game1.stats.DaysPlayed);
                    var howMuch = random.Next(1, 2) + Game1.player.ForagingLevel / 4;
                    for (var index = 0; index < howMuch; ++index)
                    {
                        var obj = ItemRegistry.Create(shakeOffItem);
                        if (Game1.player.professions.Contains(16))
                        {
                            obj.Quality = 4;
                        }
                        Game1.createItemDebris(obj, Utility.PointToVector2(bush.getBoundingBox().Center), Game1.random.Next(1, 4));
                    }
                    Game1.player.gainExperience(2, howMuch);
                    break;
            }
            if (bush.size.Value == 3)
            {
                return;
            }
            DelayedAction.playSoundAfterDelay("leafrustle", 100);
        }

        private static void GetJunimoPlush(Bush __instance)
        {
            Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Furniture("1733", Vector2.Zero), __instance.junimoPlushCallback);
        }

        public static bool MgThief_AfterSpeech_WinterMysteryFinished_Prefix(Town __instance)
        {
            try
            {
                var afterGlassMethod = _helper.Reflection.GetMethod(__instance, "mgThief_afterGlass");
                Game1.player.removeQuest("31");
                _locationChecker.AddCheckedLocation("A Winter Mystery");
                afterGlassMethod.Invoke();
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MgThief_AfterSpeech_WinterMysteryFinished_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override void populateClickableComponentList()
        public static void PopulateClickableComponentList_BearKnowledge_Postfix(PowersTab __instance)
        {
            try
            {
                var hasBearKnowledge = _archipelago.HasReceivedItem(APItem.BEARS_KNOWLEDGE);
                foreach (var powersLine in __instance.powers)
                {
                    foreach (var powerComponent in powersLine)
                    {
                        // I couldn't really find a better way to identify this icon uniquely.
                        // Ideally, in the long term, the condition should be changed in the Data itself, instead of this jank patching.
                        if (powerComponent.sourceRect.X != 192 || powerComponent.sourceRect.Y != 336)
                        {
                            continue;
                        }

                        // drawShadow is poorly named. If drawShadow, then it's "real", otherwise it's a black outline.
                        powerComponent.drawShadow = hasBearKnowledge;
                        return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PopulateClickableComponentList_BearKnowledge_Postfix)}:\n{ex}");
                return;
            }
        }

        public static void GetPriceAfterMultipliers_BearKnowledge_Postfix(Object __instance, float startPrice, long specificPlayerID, ref float __result)
        {
            try
            {
                if (__instance.QualifiedItemId != QualifiedItemIds.SALMONBERRY && __instance.QualifiedItemId != QualifiedItemIds.BLACKBERRY)
                {
                    return;
                }

                var hasSeenBearEvent = Game1.player.eventsSeen.Contains(EventIds.BEAR_KNOWLEDGE);
                var hasReceivedBearKnowledge = _archipelago.HasReceivedItem(APItem.BEARS_KNOWLEDGE);
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
                _logger.LogError($"Failed in {nameof(GetPriceAfterMultipliers_BearKnowledge_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static void AwardFestivalPrize(Event @event, string[] args, EventContext context)
        public static bool AwardFestivalPrize_QiMilk_Prefix(Event @event, string[] args, EventContext context)
        {
            try
            {
                if (args.Length < 2)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }
                var prize = args[1];
                if (!prize.Equals("qimilk", StringComparison.InvariantCultureIgnoreCase))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled && !_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.SecretNotes))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _locationChecker.AddCheckedLocation("Cryptic Note");
                _locationChecker.AddCheckedLocation(SecretsLocationNames.SECRET_NOTE_10);

                if (!Game1.player.mailReceived.Contains("qiCave"))
                {
                    Game1.player.mailReceived.Add("qiCave");
                }

                if (_archipelago.SlotData.Goal == Goal.CrypticNote)
                {
                    _archipelago.ReportGoalCompletion();
                }

                ++@event.CurrentCommand;

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AwardFestivalPrize_QiMilk_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void UpdateIgnoredQuestList()
        {
            _ignoredQuests.AddRange(IgnoredModdedStrings.Quests);
        }
    }
}
