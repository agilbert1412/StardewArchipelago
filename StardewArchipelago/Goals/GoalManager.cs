using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Goals
{
    public class GoalManager
    {
        private static LogHandler _logger;
        private IModHelper _modHelper;
        private readonly Harmony _harmony;
        private readonly StardewArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public GoalManager(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public void CheckGoalCompletion(bool vanillaGoal = false)
        {
            EvaluateGrandpaToday(Game1.getFarm());
            switch (_archipelago.SlotData.Goal)
            {
                case Goal.CommunityCenter:
                    GoalCodeInjection.CheckCommunityCenterGoalCompletion();
                    return;
                case Goal.GrandpaEvaluation:
                    GoalCodeInjection.CheckGrandpaEvaluationGoalCompletion();
                    return;
                case Goal.BottomOfMines:
                    GoalCodeInjection.CheckBottomOfTheMinesGoalCompletion();
                    return;
                case Goal.CrypticNote:
                    GoalCodeInjection.CheckCrypticNoteGoalCompletion();
                    return;
                case Goal.MasterAngler:
                    GoalCodeInjection.CheckMasterAnglerGoalCompletion(vanillaGoal);
                    return;
                case Goal.CompleteCollection:
                    GoalCodeInjection.CheckCompleteCollectionGoalCompletion();
                    return;
                case Goal.FullHouse:
                    GoalCodeInjection.CheckFullHouseGoalCompletion();
                    return;
                case Goal.GreatestWalnutHunter:
                    GoalCodeInjection.CheckWalnutHunterGoalCompletion();
                    return;
                case Goal.ProtectorOfTheValley:
                    GoalCodeInjection.CheckProtectorOfTheValleyGoalCompletion(vanillaGoal);
                    return;
                case Goal.FullShipment:
                    GoalCodeInjection.CheckFullShipmentGoalCompletion(vanillaGoal);
                    return;
                case Goal.GourmetChef:
                    GoalCodeInjection.CheckGourmetChefGoalCompletion();
                    return;
                case Goal.CraftMaster:
                    GoalCodeInjection.CheckCraftMasterGoalCompletion();
                    return;
                case Goal.Legend:
                    GoalCodeInjection.CheckLegendGoalCompletion();
                    return;
                case Goal.MysteryOfTheStardrops:
                    GoalCodeInjection.CheckMysteryOfTheStardropsGoalCompletion();
                    return;
                case Goal.Allsanity:
                    GoalCodeInjection.CheckAllsanityGoalCompletion();
                    return;
                case Goal.Perfection:
                    GoalCodeInjection.CheckPerfectionGoalCompletion();
                    return;
                default:
                    throw new ArgumentOutOfRangeException($"Goal [{_archipelago.SlotData.Goal}] is not supported in this version of the mod.");
            }
        }

        public void InjectGoalMethods()
        {
            switch (_archipelago.SlotData.Goal)
            {
                case Goal.CommunityCenter:
                    InjectCommunityCenterGoalMethods();
                    return;
                case Goal.GrandpaEvaluation:
                    return;
                case Goal.BottomOfMines:
                    InjectBottomOfTheMinesGoalMethods();
                    return;
                case Goal.CrypticNote:
                    // Gets tested on quest completion
                    return;
                case Goal.MasterAngler:
                    // Gets tested on fish caught
                    return;
                case Goal.CompleteCollection:
                    // Gets tested on donation
                    return;
                case Goal.FullHouse:
                    return;
                case Goal.GreatestWalnutHunter:
                    InjectWalnutHunterGoalMethods();
                    return;
                case Goal.ProtectorOfTheValley:
                    // Gets tested when slaying monsters
                    return;
                case Goal.FullShipment:
                    // Gets tested when Shipping an item
                    return;
                case Goal.GourmetChef:
                    // Gets tested when Cooking a recipe
                    return;
                case Goal.CraftMaster:
                    InjectCraftMasterGoalMethods();
                    return;
                case Goal.Legend:
                    InjectLegendGoalMethods();
                    return;
                case Goal.MysteryOfTheStardrops:
                    InjectStardropsGoalMethods();
                    return;
                case Goal.Allsanity:
                    // Gets tested when sending a check
                    return;
                case Goal.Perfection:
                    InjectPerfectionGoalMethods();
                    return;
                default:
                    throw new ArgumentOutOfRangeException($"Goal [{_archipelago.SlotData.Goal}] is not supported in this version of the mod.");
            }
        }

        private void InjectCommunityCenterGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "doAreaCompleteReward"),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.DoAreaCompleteReward_CommunityCenterGoal_PostFix))
            );
        }

        private void InjectBottomOfTheMinesGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.EnterMine_Level120Goal_PostFix))
            );
        }

        private void InjectWalnutHunterGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.foundWalnut)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.FoundWalnut_WalnutHunterGoal_Postfix))
            );
        }

        private void InjectCraftMasterGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe"),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.ClickCraftingRecipe_CraftMasterGoal_Postfix))
            );
        }

        private void InjectLegendGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.PropertySetter(typeof(Farmer), nameof(Farmer.totalMoneyEarned)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.TotalMoneyEarned_CheckLegendGoalCompletion_Postfix))
            );
        }

        private void InjectStardropsGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.foundAllStardrops)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.FoundAllStardrops_CheckStardropsGoalCompletion_Postfix))
            );
        }

        private void InjectPerfectionGoalMethods()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.percentGameComplete)),
                postfix: new HarmonyMethod(typeof(GoalCodeInjection), nameof(GoalCodeInjection.PercentGameComplete_PerfectionGoal_Postfix))
            );
        }

        private void EvaluateGrandpaToday(Farm farm)
        {
            var score = Utility.getGrandpaScore();
            var candlesFromScore = Utility.getGrandpaCandlesFromScore(score);
            farm.grandpaScore.Value = candlesFromScore;
            for (var index = 0; index < candlesFromScore; ++index)
            {
                DelayedAction.playSoundAfterDelay("fireball", 100 * index);
            }
            farm.addGrandpaCandles();
            AddArchipelagoIconsForPoints(farm, score);
        }

        private void AddArchipelagoIconsForPoints(Farm farm, int score)
        {
            var preference = ModEntry.Instance.Config.ShowGrandpaShrineIndicators;
            if (preference == GrandpaShrinePreferenrce.Never)
            {
                return;
            }
            if (preference == GrandpaShrinePreferenrce.GrandpaGoal && _archipelago.SlotData.Goal != Goal.GrandpaEvaluation)
            {
                return;
            }

            var size = 24;
            var whiteArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(_logger, _modHelper, size, ArchipelagoTextures.WHITE);
            var coloredArchipelagoIcon = ArchipelagoTextures.GetArchipelagoLogo(_logger, _modHelper, size, ArchipelagoTextures.COLOR);

            var grandpaShrinePosition = farm.GetGrandpaShrinePosition();
            var localId = 6666 + _archipelago.SlotData.Seed.GetHashCode();
            farm.removeTemporarySpritesWithIDLocal(localId);

            for (var i = 1; i <= 21; i++)
            {
                var texture = whiteArchipelagoIcon;
                var textureName = $"{ArchipelagoTextures.WHITE}_{size}";
                if (score >= i)
                {
                    texture = coloredArchipelagoIcon;
                    textureName = $"{ArchipelagoTextures.COLOR}_{size}";
                }

                var sprite = new TemporaryAnimatedSprite();
                sprite.texture = texture;
                sprite.textureName = textureName;

                var sourceRect = new Rectangle(0, 0, size, size);
                var column = (i - 1) % 7;
                var row = (i - 1) / 7;
                var positionX = ((grandpaShrinePosition.X - 2) * 64) + 50 + (column * 32);
                var positionY = ((grandpaShrinePosition.Y - 3) * 64) + 8 + (row * 32);
                var position = new Vector2(positionX, positionY);

                sprite.currentParentTileIndex = 0;
                sprite.initialParentTileIndex = 0;
                sprite.position = position;
                sprite.flicker = false;
                sprite.flipped = false;

                sprite.sourceRect = sourceRect;
                sprite.sourceRectStartingPos = new Vector2(sourceRect.X, sourceRect.Y);
                sprite.initialPosition = position;
                sprite.alphaFade = 0.0f;
                sprite.color = Color.White;

                sprite.interval = 50f;
                sprite.totalNumberOfLoops = 99999;
                sprite.animationLength = 1;
                sprite.lightId = $"Farm_GrandpaArchipelagoScore_{i}";
                sprite.id = localId;
                sprite.lightRadius = 1f;
                sprite.scale = 1f;
                sprite.layerDepth = 0.03f;
                sprite.delayBeforeAnimationStart = i * 50;

                farm.temporarySprites.Add(sprite);
            }
        }
    }
}
