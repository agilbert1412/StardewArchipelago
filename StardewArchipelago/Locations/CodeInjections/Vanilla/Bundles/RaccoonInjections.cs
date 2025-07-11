﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network.NetEvents;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public static class RaccoonInjections
    {
        private const string GIANT_STUMP = "Quest: The Giant Stump";

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static LocationChecker _locationChecker;
        private static BundlesManager _bundlesManager;
        private static BundleReader _bundleReader;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state,
            LocationChecker locationChecker, BundlesManager bundlesManager, BundleReader bundleReader)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
            _bundlesManager = bundlesManager;
            _bundleReader = bundleReader;
        }

        // public override bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_CheckStump_Prefix(Forest __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action.Length <= 0 || action[0] != "FixRaccoonStump" || !Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!_locationChecker.IsLocationMissing(GIANT_STUMP))
                {
                    Game1.drawObjectDialogue("Eventually someone will move in...");
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (who.Items.ContainsId("(O)709", 100))
                {
                    __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Question"), __instance.createYesNoResponses(), "ForestStump");
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:FixRaccoonStump_Hint"));
                    if (!who.mailReceived.Contains("checkedRaccoonStump"))
                    {
                        who.addQuest("134");
                        who.mailReceived.Add("checkedRaccoonStump");
                    }
                }

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_CheckStump_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_FixStump_Prefix(Forest __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "ForestStump_Yes")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (_locationChecker.IsLocationChecked(GIANT_STUMP))
                {
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                Game1.globalFadeToBlack(() => FadedForStumpFix(__instance));
                Game1.player.Items.ReduceId("(O)709", 100);
                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_FixStump_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void FadedForStumpFix(Forest forest)
        {
            Game1.freezeControls = true;
            DelayedAction.playSoundAfterDelay("crafting", 1000);
            DelayedAction.playSoundAfterDelay("crafting", 1500);
            DelayedAction.playSoundAfterDelay("crafting", 2000);
            DelayedAction.playSoundAfterDelay("crafting", 2500);
            DelayedAction.playSoundAfterDelay("axchop", 3000);
            DelayedAction.playSoundAfterDelay("discoverMineral", 3200);
            Game1.viewportFreeze = true;
            Game1.viewport.X = -10000;
            Game1.pauseThenDoFunction(4000, forest.doneWithStumpFix);
            _locationChecker.AddCheckedLocation(GIANT_STUMP);
            forest.stumpFixed.Value = true;
            Forest.fixStump(forest);
            Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.HasQuest, PlayerActionTarget.All, QuestIds.GIANT_STUMP, false);
        }

        // public void activate()
        public static bool Activate_DisplayDialogueOrBundle_Prefix(Raccoon __instance)
        {
            try
            {
                if (__instance.mrs_raccoon.Value)
                {
                    Utility.TryOpenShopMenu(nameof(Raccoon), __instance.Name);
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var maxNumberOfRaccoons = 9;
                var receivedRaccoons = _archipelago.GetReceivedItemCount(APItem.PROGRESSIVE_RACCOON);
                if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
                {
                    receivedRaccoons += 1;
                    maxNumberOfRaccoons -= 1;
                }

                var nextRaccoonRequestNumber = GetCurrentRaccoonBundleNumber();
                var raccoonBundleAvailable = nextRaccoonRequestNumber < receivedRaccoons;

                if (receivedRaccoons >= maxNumberOfRaccoons && nextRaccoonRequestNumber == -1)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                // private bool wasTalkedTo;
                var wasTalkedToField = _modHelper.Reflection.GetField<bool>(__instance, "wasTalkedTo");
                var wasTalkedTo = wasTalkedToField.GetValue();

                if (!wasTalkedTo)
                {
                    if (raccoonBundleAvailable)
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_intro"));
                    }
                    else
                    {
                        Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_interim"));
                    }
                    if (!raccoonBundleAvailable)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    Game1.afterDialogues = () => ActivateRaccoonBundleMutex(__instance);
                }
                else
                {
                    if (!raccoonBundleAvailable)
                    {
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    ActivateRaccoonBundleMutex(__instance);
                }
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Activate_DisplayDialogueOrBundle_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ActivateRaccoonBundleMutex(Raccoon raccoon)
        {
            raccoon.mutex.RequestLock(() => ActivateMrRaccoon(raccoon), () => Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_busy")));
        }

        // private void _activateMrRaccoon()
        private static void ActivateMrRaccoon(Raccoon raccoon)
        {
            // private bool wasTalkedTo;
            var wasTalkedToField = _modHelper.Reflection.GetField<bool>(raccoon, "wasTalkedTo");
            wasTalkedToField.SetValue(true);

            var currentRaccoonBundleNumber = GetCurrentRaccoonBundleNumber();
            if (currentRaccoonBundleNumber <= 0)
            {
                return;
            }

            if (Game1.netWorldState.Value.SeasonOfCurrentRacconBundle != currentRaccoonBundleNumber)
            {
                _state.CurrentRaccoonBundleStatus.Clear();
                Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = currentRaccoonBundleNumber;
            }

            var ingredients = new List<BundleIngredientDescription>();
            var raccoonRequestsRoom = _bundlesManager.BundleRooms.Rooms[APName.RACCOON_REQUESTS_ROOM];
            var currentRaccoonBundleName = $"{APName.RACCOON_REQUEST_PREFIX}{currentRaccoonBundleNumber}";
            var raccoonBundle = (ItemBundle)raccoonRequestsRoom.Bundles[currentRaccoonBundleName];
            for (var i = 0; i < raccoonBundle.Items.Count; i++)
            {
                if (_state.CurrentRaccoonBundleStatus.Count <= i)
                {
                    _state.CurrentRaccoonBundleStatus.Add(false);
                }
                if (raccoonBundle.Items[i] is null)
                {
                    throw new ArgumentException($"The raccoon must only have item bundles");
                }
                var bundleIngredient = raccoonBundle.Items[i].CreateBundleIngredientDescription(_state.CurrentRaccoonBundleStatus[i]);
                ingredients.Add(bundleIngredient);
            }

            var whichBundle = (currentRaccoonBundleNumber - 1) % 5;
            var bundle = new ArchipelagoBundle(currentRaccoonBundleName, null, ingredients, new bool[1])
            {
                BundleTextureOverride = Game1.content.Load<Texture2D>("LooseSprites\\BundleSprites"),
                BundleTextureIndexOverride = 14 + whichBundle,
                NumberOfIngredientSlots = raccoonBundle.NumberRequired,
            };
            var raccoonNoteMenu = new ArchipelagoJunimoNoteMenu(bundle, "LooseSprites\\raccoon_bundle_menu");
            raccoonNoteMenu.OnIngredientDeposit = x => _state.CurrentRaccoonBundleStatus[x] = true;
            raccoonNoteMenu.OnBundleComplete = _ => BundleComplete(raccoon);
            raccoonNoteMenu.OnScreenSwipeFinished = _ => BundleCompleteAfterSwipe(raccoon);
            raccoonNoteMenu.behaviorBeforeCleanup = _ => raccoon.mutex?.ReleaseLock();
            Game1.activeClickableMenu = raccoonNoteMenu;
        }

        // private void bundleComplete(JunimoNoteMenu menu)
        private static void BundleComplete(Raccoon raccoon)
        {
            JunimoNoteMenu.screenSwipe = new ScreenSwipe(1);
            _locationChecker.AddCheckedLocation($"{APName.RACCOON_REQUEST_PREFIX}{Game1.netWorldState.Value.SeasonOfCurrentRacconBundle}");
            Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = -1;
            _state.CurrentRaccoonBundleStatus.Clear();

            // private bool wasTalkedTo;
            var wasTalkedToField = _modHelper.Reflection.GetField<bool>(raccoon, "wasTalkedTo");
            wasTalkedToField.SetValue(false);
        }

        // private void bundleCompleteAfterSwipe(JunimoNoteMenu menu)
        private static void BundleCompleteAfterSwipe(Raccoon raccoon)
        {
            Game1.activeClickableMenu = null;
            raccoon.mutex?.ReleaseLock();
            Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.Date.TotalDays;
            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Raccoon_receive"));
        }

        private static int GetCurrentRaccoonBundleNumber()
        {
            var remainingRacoonBundleNumbers = _locationChecker.GetAllMissingLocationNames()
                .Where(x => x.StartsWith(APName.RACCOON_REQUEST_PREFIX))
                .Select(x => int.Parse(x[APName.RACCOON_REQUEST_PREFIX.Length..]));
            if (remainingRacoonBundleNumbers.Any())
            {
                return remainingRacoonBundleNumbers.Min();
            }

            return -1;
        }

        // public bool IsValidItemForThisIngredientDescription(Item item,BundleIngredientDescription ingredient)
        /*public static bool IsValidItemForThisIngredientDescription_TestPatch_Prefix(Bundle __instance, Item item, BundleIngredientDescription ingredient, ref bool __result)
        {
            try
            {
                if (item == null || ingredient.completed || ingredient.quality > item.Quality)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (ingredient.preservesId != null)
                {
                    var flavoredIngredientQuery = "FLAVORED_ITEM " + ingredient.id + " " + ingredient.preservesId;
                    var queryContext = new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random);
                    var resolvedIngredientQueryResult = ItemQueryResolver.TryResolve(flavoredIngredientQuery, queryContext);
                    var resolvedIngredient = resolvedIngredientQueryResult.FirstOrDefault()?.Item;
                    if (resolvedIngredient is Object ingredientObject && item is Object itemObject && itemObject.preservedParentSheetIndex?.Value != null)
                    {
                        var qualifiedIdsMatch = item.QualifiedItemId == ingredientObject.QualifiedItemId;
                        var preservesIdMatch = itemObject.preservedParentSheetIndex.Value.Contains(ingredient.preservesId);
                        __result = qualifiedIdsMatch && preservesIdMatch;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }

                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(IsValidItemForThisIngredientDescription_TestPatch_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }*/

        // public override void draw(SpriteBatch spriteBatch)
        public static void Draw_TreeStumpFix_Postfix(Forest __instance, SpriteBatch spriteBatch)
        {
            try
            {
                if (!_locationChecker.IsLocationMissing("Quest: The Giant Stump") || !Game1.player.hasQuest(QuestIds.GIANT_STUMP))
                {
                    return;
                }

                var num = (float)(4.0 * Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) - 8.0);
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3576f, 272f + num)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0504009947f);
                spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(3616f, 312f + num)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0.0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.050409995f);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_TreeStumpFix_Postfix)}:\n{ex}");
                return;
            }
        }

        // protected override void resetSharedState()
        public static void ResetSharedState_WalkThroughRaccoons_Postfix(Forest __instance)
        {
            try
            {
                if (!_locationChecker.IsLocationMissing("Quest: The Giant Stump"))
                {
                    return;
                }

                foreach (var character in __instance.characters)
                {
                    if (character is Raccoon raccoon)
                    {
                        raccoon.farmerPassesThrough = true;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ResetSharedState_WalkThroughRaccoons_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
