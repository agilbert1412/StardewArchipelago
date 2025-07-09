using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Quests;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity;
using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.GingerIsland;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Arcade;
using StardewArchipelago.Locations.Secrets;
using StardewValley.Buildings;
using StardewValley.Tools;
using xTile.Dimensions;
using EventInjections = StardewArchipelago.Locations.CodeInjections.Vanilla.EventInjections;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace StardewArchipelago.Locations.Patcher
{
    public class VanillaLocationPatcher : ILocationPatcher
    {
        private readonly ILogger _logger;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly Harmony _harmony;
        private readonly IModHelper _modHelper;
        private readonly GingerIslandPatcher _gingerIslandPatcher;
        private readonly ToolShopStockModifier _toolUpgradesShopStockModifier;
        private readonly FishingRodShopStockModifier _fishingRodShopStockModifier;
        private readonly CarpenterShopStockModifier _carpenterShopStockModifier;
        private readonly CarpenterBuildingsModifier _carpenterBuildingsModifier;
        private readonly AdventureGuildShopStockModifier _guildShopStockModifier;
        private readonly TravelingMerchantShopStockModifier _travelingMerchantShopStockModifier;
        private readonly FestivalShopStockModifier _festivalShopStockModifier;
        private readonly CookingRecipePurchaseStockModifier _cookingRecipePurchaseStockModifier;
        private readonly CraftingRecipePurchaseStockModifier _craftingRecipePurchaseStockModifier;
        private readonly KrobusShopStockModifier _krobusShopStockModifier;
        private readonly BookShopStockModifier _bookShopStockModifier;
        private readonly QiGemShopStockModifier _qiGemShopStockModifier;
        private readonly CataloguesShopStockModifier _catalogueShopStockModifier;

        public VanillaLocationPatcher(ILogger logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _harmony = harmony;
            _modHelper = modHelper;
            _gingerIslandPatcher = new GingerIslandPatcher(logger, _modHelper, _harmony, _archipelago, locationChecker);
            _toolUpgradesShopStockModifier = new ToolShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _fishingRodShopStockModifier = new FishingRodShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _carpenterShopStockModifier = new CarpenterShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _carpenterBuildingsModifier = new CarpenterBuildingsModifier(logger, modHelper, archipelago);
            _guildShopStockModifier = new AdventureGuildShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _travelingMerchantShopStockModifier = new TravelingMerchantShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _festivalShopStockModifier = new FestivalShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _cookingRecipePurchaseStockModifier = new CookingRecipePurchaseStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _craftingRecipePurchaseStockModifier = new CraftingRecipePurchaseStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _krobusShopStockModifier = new KrobusShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _bookShopStockModifier = new BookShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _qiGemShopStockModifier = new QiGemShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            _catalogueShopStockModifier = new CataloguesShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
        }

        public void ReplaceAllLocationsRewardsWithChecks()
        {
            try
            {
                PatchCommunityCenter();
                PatchTrashBear();
                ReplaceBackPackUpgradesWithChecks();
                ReplaceMineshaftChestsWithChecks();
                ReplaceElevatorsWithChecks();
                ReplaceToolUpgradesWithChecks();
                PatchFishingRods();
                PatchCopperPan();
                ReplaceSkillsWithChecks();
                ReplaceQuestsWithChecks();
                PatchBuildingsAndBlueprints();
                ReplaceIsolatedEventsWithChecks();
                PatchAdventurerGuildShop();
                PatchArcadeMachines();
                PatchTravelingMerchant();
                PatchFishing();
                AddMuseumsanityLocations();
                PatchFestivals();
                AddCropSanityLocations();
                ReplaceFriendshipsWithChecks();
                ReplaceSpecialOrdersWithChecks();
                ReplaceChildrenWithChecks();
                _gingerIslandPatcher.PatchGingerIslandLocations();
                PatchMonstersanity();
                AddCooksanityLocations();
                PatchChefAndCraftsanity();
                PatchConversationFriendship();
                PatchKrobusShop();
                PatchQiGemShop();
                PatchCatalogueShops();
                PatchFarmcave();
                PatchNightWorldEvents();
                PatchBooks();
                PatchWalnuts();
                PatchSecretNotes();
                PatchSecrets();
                PatchMoviesanity();
                PatchHatsanity();
                PatchEating();
                PatchEndgameLocations();
                PatchGarbageCans();
                PatchScouts();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(VanillaLocationPatcher)}.{nameof(ReplaceAllLocationsRewardsWithChecks)}. Exception: {ex}");
            }
        }

        public void CleanEvents()
        {
            CleanToolEvents();
            CleanFishingRodEvents();
            CleanCarpenterEvents();
            CleanAdventureGuildEvents();
            CleanTravelingMerchantEvents();
            CleanFestivalEvents();
            CleanChefsanityEvents();
            CleanCraftsanityEvents();
            CleanKrobusEvents();
            CleanQiGemEvents();
            CleanCatalogueShopEvents();
            CleanBookEvents();
        }

        private void PatchCommunityCenter()
        {
            ReplaceCommunityCenterBundlesWithChecks();
            ReplaceCommunityCenterAreasWithChecks();
            ReplaceRaccoonBundlesWithChecks();
            PatchMemeBundles();
        }

        private void ReplaceCommunityCenterBundlesWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), nameof(CommunityCenter.shouldNoteAppearInArea)),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.ShouldNoteAppearInArea_AllowAccessEverything_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), nameof(CommunityCenter.checkAction)),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.CheckAction_BulletinBoardNoRequirements_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "checkForMissedRewards"),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.CheckForMissedRewards_DontBother_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "resetSharedState"),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.ResetSharedState_SisyphusStoneFallDown_Postfix))
            );
        }

        private void ReplaceCommunityCenterAreasWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "doAreaCompleteReward"),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.DoAreaCompleteReward_AreaLocations_Prefix))
            );
        }

        private void ReplaceRaccoonBundlesWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Raccoon), nameof(Raccoon.activate)),
                prefix: new HarmonyMethod(typeof(RaccoonInjections), nameof(RaccoonInjections.Activate_DisplayDialogueOrBundle_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), "resetSharedState"),
                postfix: new HarmonyMethod(typeof(RaccoonInjections), nameof(RaccoonInjections.ResetSharedState_WalkThroughRaccoons_Postfix))
            );

            if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
            {
                return;
            }

            var performActionParameters = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), nameof(Forest.performAction), performActionParameters),
                prefix: new HarmonyMethod(typeof(RaccoonInjections), nameof(RaccoonInjections.PerformAction_CheckStump_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), nameof(Forest.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(RaccoonInjections), nameof(RaccoonInjections.AnswerDialogueAction_FixStump_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), nameof(Forest.draw)),
                postfix: new HarmonyMethod(typeof(RaccoonInjections), nameof(RaccoonInjections.Draw_TreeStumpFix_Postfix))
            );
        }

        private void PatchMemeBundles()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Building), nameof(Building.doAction)),
                prefix: new HarmonyMethod(typeof(WellInjections), nameof(WellInjections.DoAction_ThrowHoneyInWell_Prefix))
            );
        }

        private void PatchTrashBear()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), "resetLocalState"),
                postfix: new HarmonyMethod(typeof(TrashBearInjections), nameof(TrashBearInjections.ResetLocalState_SpawnTrashBear_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(TrashBear), "doCutscene"),
                prefix: new HarmonyMethod(typeof(TrashBearInjections), nameof(TrashBearInjections.DoCutscene_DontPlayTheCutsceneNormally_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(TrashBear), nameof(TrashBear.draw), new[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(TrashBearInjections), nameof(TrashBearInjections.Draw_DrawAllDesiredItemsAtOnce_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(TrashBear), nameof(TrashBear.tryToReceiveActiveObject)),
                prefix: new HarmonyMethod(typeof(TrashBearInjections), nameof(TrashBearInjections.TryToReceiveActiveObject_AcceptAnyDesiredItem_Prefix))
            );
        }

        private void ReplaceBackPackUpgradesWithChecks()
        {
            if (_archipelago.SlotData.BackpackProgression == BackpackProgression.Vanilla)
            {
                return;
            }

            var performActionArgumentTypes = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionArgumentTypes),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.PerformAction_BuyBackpack_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.AnswerDialogueAction_BackPackPurchase_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SeedShop), nameof(SeedShop.draw)),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.Draw_SeedShopBackpack_Prefix))
            );

            /*_harmony.Patch(
                original: AccessTools.Constructor(typeof(GameMenu), new[] { typeof(bool) }),
                postfix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.GameMenuConstructor_UseFakeInventoryPages_Postfix))
            );*/

            /*_harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.MaxItems)),
                postfix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.MaxItemsGetter_CheckAmount_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.PropertySetter(typeof(Farmer), nameof(Farmer.MaxItems)),
                postfix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.MaxItemsSetter_CheckAmount_Postfix))
            );*/
        }

        private void ReplaceMineshaftChestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.CheckForAction_MineshaftChest_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.AddLevelChests_Level120_Prefix))
            );
        }

        private void ReplaceToolUpgradesWithChecks()
        {
            var performActionArgumentTypes = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionArgumentTypes),
                prefix: new HarmonyMethod(typeof(ScytheInjections), nameof(ScytheInjections.PerformAction_GoldenScythe_Prefix))
            );

            _modHelper.Events.Content.AssetRequested += _toolUpgradesShopStockModifier.OnShopStockRequested;
        }

        private void CleanToolEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _toolUpgradesShopStockModifier.OnShopStockRequested;
        }

        private void PatchFishingRods()
        {
            _modHelper.Events.Content.AssetRequested += _fishingRodShopStockModifier.OnShopStockRequested;

            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.SkipEvent_BambooPole_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.AwardFestivalPrize_BambooPole_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.SkipEvent_WillyFishingLesson_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.AwardFestivalPrize_WillyFishingLesson_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.GainSkill)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.GainSkill_WillyFishingLesson_Prefix))
            );
        }

        private void PatchCopperPan()
        {
            if (!_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.Progressive))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(CopperPanInjections), nameof(CopperPanInjections.SkipEvent_CopperPan_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(CopperPanInjections), nameof(CopperPanInjections.AwardFestivalPrize_CopperPan_Prefix))
            );
        }

        private void CleanFishingRodEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _fishingRodShopStockModifier.OnShopStockRequested;
        }

        private void ReplaceElevatorsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineElevatorMenu), nameof(MineElevatorMenu.draw), new[] { typeof(SpriteBatch) }),
                postfix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.Draw_AddArchipelagoIndicators_Postfix))
            );

            if (_archipelago.SlotData.ElevatorProgression == ElevatorProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.EnterMine_SendElevatorCheck_PostFix))
            );

            var performActionArgumentTypes = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionArgumentTypes),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.PerformAction_LoadElevatorMenu_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.CheckAction_LoadElevatorMenu_Prefix))
            );
        }

        private void ReplaceSkillsWithChecks()
        {
            if (_archipelago.SlotData.SkillProgression == SkillsProgression.Vanilla)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                    prefix: new HarmonyMethod(typeof(SkillInjections), nameof(SkillInjections.GainExperience_NormalExperience_Prefix))
                );
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                prefix: new HarmonyMethod(typeof(SkillInjections), nameof(SkillInjections.GainExperience_ArchipelagoExperience_Prefix))
            );

            var performActionTypes = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            if (_archipelago.SlotData.SkillProgression != SkillsProgression.ProgressiveWithMasteries)
            {

                _harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionTypes),
                    prefix: new HarmonyMethod(typeof(MasteriesInjections), nameof(MasteriesInjections.PerformAction_VanillaMasteryCaveInteractions_Prefix))
                );
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionTypes),
                prefix: new HarmonyMethod(typeof(MasteriesInjections), nameof(MasteriesInjections.PerformAction_MasteryCaveInteractions_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), "claimReward"),
                prefix: new HarmonyMethod(typeof(MasteriesInjections), nameof(MasteriesInjections.ClaimReward_SendMasteryCheck_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.hasCompletedAllMasteryPlaques)),
                prefix: new HarmonyMethod(typeof(MasteriesInjections), nameof(MasteriesInjections.HasCompletedAllMasteryPlaques_RelyOnSentChecks_Prefix))
            );
        }

        private void ReplaceQuestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Railroad), "resetLocalState"),
                prefix: new HarmonyMethod(typeof(DarkTalismanInjections), nameof(DarkTalismanInjections.ResetLocalState_PlayCutsceneIfConditionsAreMet_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Quest), nameof(Quest.questComplete)),
                prefix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.QuestComplete_LocationInsteadOfReward_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.AwardFestivalPrize_QiMilk_Prefix))
            );

            var performActionArgumentTypes = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionArgumentTypes),
                prefix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.PerformAction_MysteriousQiLumberPile_Prefix))
            );

            if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.RemoveQuest)),
                postfix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.RemoveQuest_CheckLocation_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getQuestOfTheDay)),
                prefix: new HarmonyMethod(typeof(HelpWantedQuestInjections), nameof(HelpWantedQuestInjections.GetQuestOfTheDay_BalanceQuests_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getRandomItemFromSeason), new[] { typeof(Season), typeof(bool), typeof(Random) }),
                postfix: new HarmonyMethod(typeof(HelpWantedQuestInjections), nameof(HelpWantedQuestInjections.GetRandomItemFromSeason_RemoveFishIfCantCatchThem_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Bush), "shake"),
                prefix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.Shake_WinterMysteryBush_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Town), "mgThief_afterSpeech"),
                prefix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.MgThief_AfterSpeech_WinterMysteryFinished_Prefix))
            );

            PatchPowersTab();

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), "getPriceAfterMultipliers"),
                postfix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.GetPriceAfterMultipliers_BearKnowledge_Postfix))
            );

            ReplaceDarkTalismanQuestsWithChecks();
        }

        private void ReplaceDarkTalismanQuestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialItem), nameof(SpecialItem.actionWhenReceived)),
                postfix: new HarmonyMethod(typeof(DarkTalismanInjections), nameof(DarkTalismanInjections.ActionWhenReceived_MagicInk_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(DarkTalismanInjections), nameof(DarkTalismanInjections.CheckForAction_BuglandChest_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "performRemoveHenchman"),
                prefix: new HarmonyMethod(typeof(DarkTalismanInjections), nameof(DarkTalismanInjections.PerformRemoveHenchman_CheckGoblinProblemLocation_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.setUpLocationSpecificFlair)),
                prefix: new HarmonyMethod(typeof(DarkTalismanInjections), nameof(DarkTalismanInjections.SetUpLocationSpecificFlair_CreateBuglandChest_Prefix))
            );
        }

        private void PatchPowersTab()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(PowersTab), nameof(PowersTab.populateClickableComponentList)),
                postfix: new HarmonyMethod(typeof(StoryQuestInjections), nameof(StoryQuestInjections.PopulateClickableComponentList_DisplayPowersReceivedFromAp_Postfix))
            );
        }

        private void ReplaceSpecialOrdersWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.GetSpecialOrder)),
                postfix: new HarmonyMethod(typeof(SpecialOrderInjections), nameof(SpecialOrderInjections.GetSpecialOrder_ArchipelagoReward_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.CheckCompletion)),
                postfix: new HarmonyMethod(typeof(SpecialOrderInjections), nameof(SpecialOrderInjections.CheckCompletion_ArchipelagoReward_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.SetDuration)),
                prefix: new HarmonyMethod(typeof(SpecialOrderInjections), nameof(SpecialOrderInjections.SetDuration_UseCorrectDateWithSeasonRandomizer_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.UpdateAvailableSpecialOrders)),
                prefix: new HarmonyMethod(typeof(SpecialOrderInjections), nameof(SpecialOrderInjections.UpdateAvailableSpecialOrders_ChangeFrequencyToBeLessRng_Prefix))
            );

            if (!_archipelago.SlotData.SpecialOrderLocations.HasFlag(SpecialOrderLocations.Board))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrder), nameof(SpecialOrder.IsSpecialOrdersBoardUnlocked)),
                prefix: new HarmonyMethod(typeof(SpecialOrderInjections), nameof(SpecialOrderInjections.IsSpecialOrdersBoardUnlocked_UnlockBasedOnApItem_Prefix))
            );
        }

        private void ReplaceChildrenWithChecks()
        {
            if (_archipelago.SlotData.Friendsanity == Friendsanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
                prefix: new HarmonyMethod(typeof(SpouseInjections), nameof(SpouseInjections.CheckAction_SpouseStardrop_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.canGetPregnant)),
                prefix: new HarmonyMethod(typeof(PregnancyInjections), nameof(PregnancyInjections.CanGetPregnant_ShuffledPregnancies_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
                prefix: new HarmonyMethod(typeof(PregnancyInjections), nameof(PregnancyInjections.Setup_PregnancyQuestionEvent_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(QuestionEvent), "answerPregnancyQuestion"),
                prefix: new HarmonyMethod(typeof(PregnancyInjections), nameof(PregnancyInjections.AnswerPregnancyQuestion_CorrectDate_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
                prefix: new HarmonyMethod(typeof(PregnancyInjections), nameof(PregnancyInjections.TickUpdate_BirthingEvent_Prefix))
            );
        }

        private void PatchBuildingsAndBlueprints()
        {
            _modHelper.Events.Content.AssetRequested += _carpenterShopStockModifier.OnShopStockRequested;
            _modHelper.Events.Content.AssetRequested += _carpenterBuildingsModifier.OnBuildingsRequested;
            _modHelper.GameContent.InvalidateCache("Data/Buildings");

            var performActionArgumentTypes = new[] { typeof(string[]), typeof(Farmer), typeof(Location) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), performActionArgumentTypes),
                prefix: new HarmonyMethod(typeof(WizardBookInjections), nameof(WizardBookInjections.PerformAction_WizardBook_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(WizardBookInjections), nameof(WizardBookInjections.AnswerDialogueAction_WizardBook_Prefix))
            );


            _logger.LogDebug($"Attempting to patch the BlueprintEntryConstructor");
            var blueprintEntryParameters = new[] { typeof(int), typeof(string), typeof(BuildingData), typeof(string) };
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(CarpenterMenu.BlueprintEntry), blueprintEntryParameters),
                prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.BlueprintEntryConstructor_IfFreeMakeTheIdCorrect_Prefix))
            );
            _logger.LogDebug($"Finished patching the BlueprintEntryConstructor");


            if (!_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive))
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), "houseUpgradeOffer"),
                    prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.HouseUpgradeOffer_OfferCheaperUpgrade_Prefix))
                );

                _harmony.Patch(
                    original: AccessTools.Method(typeof(GameLocation), "houseUpgradeAccept"),
                    prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.HouseUpgradeAccept_CheaperInAP_Prefix))
                );

                return;
            }

            var desiredOverloadParameters = new[] { typeof(string), typeof(Response[]), typeof(string) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), desiredOverloadParameters),
                prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.CreateQuestionDialogue_CarpenterDialogOptions_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "houseUpgradeOffer"),
                prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.HouseUpgradeOffer_OfferFreeUpgrade_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "houseUpgradeAccept"),
                prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.HouseUpgradeAccept_FreeFromAP_Prefix))
            );
        }

        private void CleanCarpenterEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _carpenterShopStockModifier.OnShopStockRequested;
            _modHelper.Events.Content.AssetRequested -= _carpenterBuildingsModifier.OnBuildingsRequested;
        }

        private void ReplaceIsolatedEventsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.SkipEvent_RustySword_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.AwardFestivalPrize_RustySword_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Woods), nameof(Woods.checkAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.CheckAction_OldMasterCanolli_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.AnswerDialogueAction_BeachBridge_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.CheckAction_BeachBridge_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.fixBridge)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.FixBridge_DontFixDuringDraw_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.draw)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.Draw_BeachBridgeQuestionMark_Prefix)),
                postfix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.Draw_BeachBridgeQuestionMark_Postfix))
            );

            var performTouchActionArgumentTypes = new[] { typeof(string[]), typeof(Vector2) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction), performTouchActionArgumentTypes),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.PerformTouchAction_GalaxySwordShrine_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.CheckForAction_PotOfGold_Prefix))
            );
        }

        private void PatchAdventurerGuildShop()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(DefaultPhoneHandler), nameof(DefaultPhoneHandler.CallAdventureGuild)),
                prefix: new HarmonyMethod(typeof(PhoneInjections), nameof(PhoneInjections.CallAdventureGuild_AllowRecovery_Prefix))
            );

            _modHelper.Events.Content.AssetRequested += _guildShopStockModifier.OnShopStockRequested;
        }

        private void CleanAdventureGuildEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _guildShopStockModifier.OnShopStockRequested;
        }

        private void PatchArcadeMachines()
        {
            PatchJourneyOfThePrairieKing();
            PatchJunimoKart();
        }

        private void PatchJourneyOfThePrairieKing()
        {
            if (_archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.usePowerup)),
                prefix: new HarmonyMethod(typeof(JotPKInjections), nameof(JotPKInjections.UsePowerup_PrairieKingBossBeaten_Prefix))
            );

            if (_archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.Victories)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame.CowboyMonster), nameof(AbigailGame.CowboyMonster.getLootDrop)),
                prefix: new HarmonyMethod(typeof(JotPKInjections), nameof(JotPKInjections.GetLootDrop_ExtraLoot_Prefix))
            );

            var desiredAbigailGameCtorParameters = new[] { typeof(NPC) };
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(AbigailGame), desiredAbigailGameCtorParameters),
                postfix: new HarmonyMethod(typeof(JotPKInjections), nameof(JotPKInjections.AbigailGameCtor_Equipments_Postfix))
            );

            if (_archipelago.SlotData.ArcadeMachineLocations != ArcadeLocations.FullShuffling)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.startShoppingLevel)),
                postfix: new HarmonyMethod(typeof(JotPKInjections), nameof(JotPKInjections.StartShoppingLevel_ShopBasedOnSentChecks_PostFix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.tick)),
                postfix: new HarmonyMethod(typeof(JotPKInjections), nameof(JotPKInjections.Tick_Shopping_PostFix))
            );
        }

        private void PatchJunimoKart()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NetLeaderboards), nameof(NetLeaderboards.GetScores)),
                prefix: new HarmonyMethod(typeof(JunimoKartInjections), nameof(JunimoKartInjections.GetScores_AddScoresFromMultiworld_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), nameof(MineCart.submitHighScore)),
                prefix: new HarmonyMethod(typeof(JunimoKartInjections), nameof(JunimoKartInjections.SubmitHighScore_AddScoreToMultiworld_Prefix))
            );

            if (_archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), nameof(MineCart.EndCutscene)),
                prefix: new HarmonyMethod(typeof(JunimoKartInjections), nameof(JunimoKartInjections.EndCutscene_JunimoKartLevelComplete_Prefix))
            );

            if (_archipelago.SlotData.ArcadeMachineLocations == ArcadeLocations.Victories)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), "restartLevel"),
                postfix: new HarmonyMethod(typeof(JunimoKartInjections), nameof(JunimoKartInjections.RestartLevel_NewGame_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), nameof(MineCart.UpdateFruitsSummary)),
                postfix: new HarmonyMethod(typeof(JunimoKartInjections), nameof(JunimoKartInjections.UpdateFruitsSummary_ExtraLives_Postfix))
            );
        }

        private void PatchTravelingMerchant()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), nameof(Forest.ShouldTravelingMerchantVisitToday)),
                prefix: new HarmonyMethod(typeof(TravelingMerchantInjections), nameof(TravelingMerchantInjections.ShouldTravelingMerchantVisitToday_ArchipelagoDays_Prefix))
            );

            // This patch only exists because apparently, on Mac, the compiler optimizes the previous patch away. That's weird.
            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), "resetSharedState"),
                postfix: new HarmonyMethod(typeof(TravelingMerchantInjections), nameof(TravelingMerchantInjections.ResetSharedState_MakeSureItDoesPatchedTravelingCart_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BeachNightMarket), nameof(BeachNightMarket.checkAction)),
                prefix: new HarmonyMethod(typeof(TravelingMerchantInjections), nameof(TravelingMerchantInjections.NightMarketCheckAction_IsTravelingMerchantDay_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BeachNightMarket), nameof(DesertFestival.checkAction)),
                prefix: new HarmonyMethod(typeof(TravelingMerchantInjections), nameof(TravelingMerchantInjections.DesertFestivalCheckAction_IsTravelingMerchantDay_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.SetUpShopOwner)),
                postfix: new HarmonyMethod(typeof(TravelingMerchantInjections), nameof(TravelingMerchantInjections.SetUpShopOwner_TravelingMerchantApFlair_Postfix))
            );

            _modHelper.Events.Content.AssetRequested += _travelingMerchantShopStockModifier.OnShopStockRequested;
        }

        private void CleanTravelingMerchantEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _travelingMerchantShopStockModifier.OnShopStockRequested;
        }

        private void PatchFishing()
        {
            AddFishsanityLocations();
            _harmony.Patch(
                original: AccessTools.Method(typeof(BobberBar), nameof(BobberBar.update)),
                prefix: new HarmonyMethod(typeof(FishingInjections), nameof(FishingInjections.Update_CountMissedFish_Prefix))
            );
        }

        private void AddFishsanityLocations()
        {
            if (_archipelago.SlotData.Goal == Goal.MasterAngler)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.caughtFish)),
                    postfix: new HarmonyMethod(typeof(FishingInjections), nameof(FishingInjections.CaughtFish_CheckGoalCompletion_Postfix))
                );
            }

            if (_archipelago.SlotData.Fishsanity == Fishsanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.caughtFish)),
                postfix: new HarmonyMethod(typeof(FishingInjections), nameof(FishingInjections.CaughtFish_Fishsanity_Postfix))
            );
        }

        private void AddMuseumsanityLocations()
        {
            if (_archipelago.SlotData.Goal == Goal.CompleteCollection)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(LibraryMuseum), nameof(LibraryMuseum.getRewardsForPlayer)),
                    postfix: new HarmonyMethod(typeof(MuseumInjections), nameof(MuseumInjections.GetRewardsForPlayer_CheckGoalCompletion_Postfix))
                );
            }

            if (_archipelago.SlotData.Museumsanity == Museumsanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(LibraryMuseum), nameof(LibraryMuseum.getRewardsForPlayer)),
                prefix: new HarmonyMethod(typeof(MuseumInjections), nameof(MuseumInjections.GetRewardsForPlayer_Museumsanity_Prefix))
            );
        }

        private void ReplaceFriendshipsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Friendship), nameof(Friendship.Points)),
                prefix: new HarmonyMethod(typeof(FriendshipInjections), nameof(FriendshipInjections.GetPoints_ArchipelagoHearts_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(SocialPage), new[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
                postfix: new HarmonyMethod(typeof(FriendshipInjections), nameof(FriendshipInjections.SocialPageCtor_CheckHints_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SocialPage), "drawNPCSlot"),
                postfix: new HarmonyMethod(typeof(FriendshipInjections), nameof(FriendshipInjections.DrawNPCSlot_DrawEarnedHearts_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Pet), nameof(Pet.dayUpdate)),
                prefix: new HarmonyMethod(typeof(FriendshipInjections), nameof(FriendshipInjections.DayUpdate_ArchipelagoPoints_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.changeFriendship)),
                prefix: new HarmonyMethod(typeof(FriendshipInjections), nameof(FriendshipInjections.ChangeFriendship_ArchipelagoPoints_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.resetFriendshipsForNewDay)),
                prefix: new HarmonyMethod(typeof(FriendshipInjections), nameof(FriendshipInjections.ResetFriendshipsForNewDay_AutopetHumans_Prefix))
            );
        }

        private void PatchFestivals()
        {
            _modHelper.Events.Content.AssetRequested += _festivalShopStockModifier.OnShopStockRequested;

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.GetRandomWinterStarParticipant)),
                prefix: new HarmonyMethod(typeof(WinterStarInjections), nameof(WinterStarInjections.GetRandomWinterStarParticipant_ChooseBasedOnMonthNotYear_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.forceEndFestival)),
                prefix: new HarmonyMethod(typeof(FairInjections), nameof(FairInjections.ForceEndFestival_KeepStarTokens_Prefix))
            );

            if (_archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla)
            {
                return;
            }

            PatchEggFestival();
            PatchDesertFestival();
            PatchFlowerDance();
            PatchLuau();
            PatchTroutDerby();
            PatchDanceOfTheMoonlightJellies();
            PatchFair();
            PatchSpiritsEve();
            PatchIceFestival();
            PatchSquidFest();
            PatchNightMarket();
            PatchWinterStar();
        }

        private void CleanFestivalEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _festivalShopStockModifier.OnShopStockRequested;

            if (_archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla)
            {
                return;
            }

            CleanWinterStarEvents();
        }

        private void PatchEggFestival()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(EggFestivalInjections), nameof(EggFestivalInjections.AwardFestivalPrize_Strawhat_Prefix))
            );
        }

        private void PatchDesertFestival()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(DesertFestival), nameof(DesertFestival.CollectRacePrizes)),
                prefix: new HarmonyMethod(typeof(DesertFestivalInjections), nameof(DesertFestivalInjections.CollectRacePrizes_RaceWinner_Prefix))
            );

            var makeoverParameters = new[] { typeof(int) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(DesertFestival), nameof(DesertFestival.ReceiveMakeOver), makeoverParameters),
                postfix: new HarmonyMethod(typeof(DesertFestivalInjections), nameof(DesertFestivalInjections.ReceiveMakeOver_EmilyServices_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(DesertFestival), nameof(DesertFestival.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(DesertFestivalInjections), nameof(DesertFestivalInjections.AnswerDialogueAction_CactusAndGil_Prefix)),
                postfix: new HarmonyMethod(typeof(DesertFestivalInjections), nameof(DesertFestivalInjections.AnswerDialogueAction_DesertChef_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "signalCalicoStatueActivation"),
                postfix: new HarmonyMethod(typeof(DesertFestivalInjections), nameof(DesertFestivalInjections.SignalCalicoStatueActivation_DesertChef_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(DesertFestival), nameof(DesertFestival.CleanupFestival)),
                prefix: new HarmonyMethod(typeof(DesertFestivalInjections), nameof(DesertFestivalInjections.CleanupFestival_LetPlayerKeepCalicoEggs_Prefix))
            );
        }

        private void PatchFlowerDance()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.setUpFestivalMainEvent)),
                postfix: new HarmonyMethod(typeof(FlowerDanceInjections), nameof(FlowerDanceInjections.SetUpFestivalMainEvent_FlowerDance_Postfix))
            );
        }

        private void PatchLuau()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.SwitchEvent)),
                postfix: new HarmonyMethod(typeof(LuauInjections), nameof(LuauInjections.SwitchEvent_GovernorReactionToSoup_Postfix))
            );
        }

        private void PatchTroutDerby()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(TroutDerbyInjections), nameof(TroutDerbyInjections.AnswerDialogueAction_TroutDerbyRewards_Prefix))
            );
        }

        private void PatchDanceOfTheMoonlightJellies()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.setUpFestivalMainEvent)),
                postfix: new HarmonyMethod(typeof(MoonlightJelliesInjections), nameof(MoonlightJelliesInjections.SetUpFestivalMainEvent_MoonlightJellies_Postfix))
            );
        }

        private void PatchFair()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(StrengthGame), nameof(StrengthGame.update)),
                prefix: new HarmonyMethod(typeof(FairInjections), nameof(FairInjections.StrengthGameUpdate_StrongEnough_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.interpretGrangeResults)),
                postfix: new HarmonyMethod(typeof(FairInjections), nameof(FairInjections.InterpretGrangeResults_Success_Postfix))
            );
        }

        private void PatchSpiritsEve()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(SpiritEveInjections), nameof(SpiritEveInjections.CheckForAction_SpiritEveChest_Prefix))
            );
        }

        private void PatchIceFestival()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(IceFestivalInjections), nameof(IceFestivalInjections.AwardFestivalPrize_FishingCompetition_Prefix))
            );
        }

        private void PatchSquidFest()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(SquidFestInjections), nameof(SquidFestInjections.AnswerDialogueAction_SquidFestRewards_Prefix))
            );
        }

        private void PatchNightMarket()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(MermaidHouse), nameof(MermaidHouse.UpdateWhenCurrentLocation)),
                postfix: new HarmonyMethod(typeof(MermaidHouseInjections), nameof(MermaidHouseInjections.UpdateWhenCurrentLocation_SongFinished_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BeachNightMarket), nameof(BeachNightMarket.draw)),
                prefix: new HarmonyMethod(typeof(BeachNightMarketInjections), nameof(BeachNightMarketInjections.Draw_DontDrawOriginalPainting_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BeachNightMarket), nameof(BeachNightMarket.draw)),
                postfix: new HarmonyMethod(typeof(BeachNightMarketInjections), nameof(BeachNightMarketInjections.Draw_DrawCorrectPainting_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BeachNightMarket), nameof(BeachNightMarket.checkAction)),
                prefix: new HarmonyMethod(typeof(BeachNightMarketInjections), nameof(BeachNightMarketInjections.CheckAction_LupiniPainting_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(BeachNightMarket), nameof(BeachNightMarket.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(BeachNightMarketInjections), nameof(BeachNightMarketInjections.AnswerDialogueAction_LupiniPainting_Prefix))
            );
        }

        private void PatchWinterStar()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.chooseResponse)),
                postfix: new HarmonyMethod(typeof(WinterStarInjections), nameof(WinterStarInjections.ChooseResponse_LegendOfTheWinterStar_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.chooseSecretSantaGift)),
                prefix: new HarmonyMethod(typeof(WinterStarInjections), nameof(WinterStarInjections.ChooseSecretSantaGift_SuccessfulGift_Prefix))
            );
            _modHelper.Events.Content.AssetRequested += WinterStarInjections.OnFestivalsRequested;
            _modHelper.GameContent.InvalidateCache("Data/Festivals");
        }

        private void CleanWinterStarEvents()
        {
            _modHelper.Events.Content.AssetRequested -= WinterStarInjections.OnFestivalsRequested;
        }

        private void AddCropSanityLocations()
        {
            if (_archipelago.SlotData.Cropsanity == Cropsanity.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
                postfix: new HarmonyMethod(typeof(CropsanityInjections), nameof(CropsanityInjections.Harvest_CheckCropsanityLocation_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.shake)),
                prefix: new HarmonyMethod(typeof(CropsanityInjections), nameof(CropsanityInjections.Shake_CheckCropsanityFruitTreeLocation_Prefix))
            );
        }

        private void PatchMonstersanity()
        {
            if (_archipelago.SlotData.Goal == Goal.ProtectorOfTheValley)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Stats), nameof(Stats.monsterKilled)),
                    postfix: new HarmonyMethod(typeof(MonsterSlayerInjections), nameof(MonsterSlayerInjections.MonsterKilled_CheckGoalCompletion_Postfix))
                );

                _harmony.Patch(
                    original: AccessTools.Method(typeof(AdventureGuild), nameof(AdventureGuild.areAllMonsterSlayerQuestsComplete)),
                    prefix: new HarmonyMethod(typeof(MonsterSlayerInjections), nameof(MonsterSlayerInjections.AreAllMonsterSlayerQuestsComplete_ExcludeGingerIsland_Prefix))
                );
            }

            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Character), nameof(Character.Name)),
                postfix: new HarmonyMethod(typeof(MonsterSlayerInjections), nameof(MonsterSlayerInjections.GetName_SkeletonMage_Postfix))
            );

            if (_archipelago.SlotData.Monstersanity == Monstersanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(AdventureGuild), "gil"),
                prefix: new HarmonyMethod(typeof(MonsterSlayerInjections), nameof(MonsterSlayerInjections.Gil_NoMonsterSlayerRewards_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(AdventureGuild), nameof(AdventureGuild.showMonsterKillList)),
                prefix: new HarmonyMethod(typeof(MonsterSlayerInjections), nameof(MonsterSlayerInjections.ShowMonsterKillList_CustomListFromAP_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Stats), nameof(Stats.monsterKilled)),
                postfix: new HarmonyMethod(typeof(MonsterSlayerInjections), nameof(MonsterSlayerInjections.MonsterKilled_SendMonstersanityCheck_Postfix))
            );
        }

        private void AddCooksanityLocations()
        {
            if (_archipelago.SlotData.Cooksanity == Cooksanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.cookedRecipe)),
                postfix: new HarmonyMethod(typeof(CookingInjections), nameof(CookingInjections.CookedRecipe_CheckCooksanityLocation_Postfix))
            );
        }

        private void PatchChefAndCraftsanity()
        {
            PatchChefsanity();
            PatchCraftsanity();

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(EventInjections), nameof(EventInjections.SkipEvent_ReplaceRecipe_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.tryEventCommand)),
                prefix: new HarmonyMethod(typeof(EventInjections), nameof(EventInjections.TryEventCommand_ReplaceRecipeWithCheck_Prefix))
            );
        }

        private void PatchChefsanity()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(TV), "getWeeklyRecipe", Type.EmptyTypes),
                prefix: new HarmonyMethod(typeof(QueenOfSauceInjections), nameof(QueenOfSauceInjections.GetWeeklyRecipe_UseArchipelagoSchedule_Prefix))
            );

            _modHelper.Events.Content.AssetRequested += _cookingRecipePurchaseStockModifier.OnShopStockRequested;

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(LevelUpMenu), new[] { typeof(int), typeof(int) }),
                prefix: new HarmonyMethod(typeof(RecipeLevelUpInjections), nameof(RecipeLevelUpInjections.LevelUpMenuConstructor_SendSkillRecipeChecks_Postfix))
            );

            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Friendship))
            {
                return;
            }
        }

        private void CleanChefsanityEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _cookingRecipePurchaseStockModifier.OnShopStockRequested;
        }

        private void PatchCraftsanity()
        {
            _modHelper.Events.Content.AssetRequested += _craftingRecipePurchaseStockModifier.OnShopStockRequested;

            if (_archipelago.SlotData.Craftsanity == Craftsanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AddCraftingRecipe)),
                prefix: new HarmonyMethod(typeof(CraftingInjections), nameof(CraftingInjections.AddCraftingRecipe_SkipLearningFurnace_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(CraftingInjections), nameof(CraftingInjections.SkipEvent_FurnaceRecipe_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Stats), nameof(Stats.checkForCraftingAchievements)),
                postfix: new HarmonyMethod(typeof(CraftingInjections), nameof(CraftingInjections.CheckForCraftingAchievements_CheckCraftsanityLocation_Postfix))
            );
        }

        private void CleanCraftsanityEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _craftingRecipePurchaseStockModifier.OnShopStockRequested;
        }

        private void PatchConversationFriendship()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.grantConversationFriendship)),
                prefix: new HarmonyMethod(typeof(ConversationFriendshipInjections), nameof(ConversationFriendshipInjections.GrantConversationFriendship_TalkEvents_Postfix))
            );
        }

        private void PatchKrobusShop()
        {
            _modHelper.Events.Content.AssetRequested += _krobusShopStockModifier.OnShopStockRequested;
        }

        private void CleanKrobusEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _krobusShopStockModifier.OnShopStockRequested;
        }

        private void PatchQiGemShop()
        {
            _modHelper.Events.Content.AssetRequested += _qiGemShopStockModifier.OnShopStockRequested;
        }

        private void CleanQiGemEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _qiGemShopStockModifier.OnShopStockRequested;
        }

        private void PatchCatalogueShops()
        {
            _modHelper.Events.Content.AssetRequested += _catalogueShopStockModifier.OnShopStockRequested;
        }

        private void CleanCatalogueShopEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _catalogueShopStockModifier.OnShopStockRequested;
        }

        private void PatchFarmcave()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.answerDialogue)),
                prefix: new HarmonyMethod(typeof(FarmCaveInjections), nameof(FarmCaveInjections.AnswerDialogue_SendFarmCaveCheck_Prefix))
            );
        }

        private void PatchNightWorldEvents()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickFarmEvent)),
                postfix: new HarmonyMethod(typeof(FarmEventInjections), nameof(FarmEventInjections.PickFarmEvent_MrQiPlaneOnlyIfUnlocked_Postfix))
            );
        }

        private void PatchBooks()
        {
            _modHelper.Events.Content.AssetRequested += _bookShopStockModifier.OnShopStockRequested;

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), "readBook"),
                prefix: new HarmonyMethod(typeof(BookInjections), nameof(BookInjections.ReadBook_Booksanity_Prefix))
            );

            if (_archipelago.SlotData.Booksanity < Booksanity.All)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.readNote)),
                prefix: new HarmonyMethod(typeof(BookInjections), nameof(BookInjections.ReadNote_BooksanityLostBook_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(LibraryMuseum), "resetLocalState"),
                prefix: new HarmonyMethod(typeof(BookInjections), nameof(BookInjections.ResetLocalState_BooksanityLostBooks_Prefix))
            );
        }

        private void CleanBookEvents()
        {
            _modHelper.Events.Content.AssetRequested -= _bookShopStockModifier.OnShopStockRequested;
        }

        private void PatchWalnuts()
        {
            PatchPuzzleWalnuts();
            PatchBushesWalnuts();
            PatchDigSpotWalnuts();
            PatchRepeatableWalnuts();

            _harmony.Patch(
                original: AccessTools.Method(typeof(FarmerTeam), nameof(FarmerTeam.RequestLimitedNutDrops)),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.RequestLimitedNutDrops_TigerSlimesAndCreatesWalnuts_Prefix))
            );
        }

        private void PatchBushesWalnuts()
        {
            if (!_archipelago.SlotData.Walnutsanity.HasFlag(Walnutsanity.Bushes))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Bush), nameof(Bush.GetShakeOffItem)),
                prefix: new HarmonyMethod(typeof(WalnutBushInjections), nameof(WalnutBushInjections.GetShakeOffItem_ReplaceWalnutWithCheck_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Bush), nameof(Bush.setUpSourceRect)),
                prefix: new HarmonyMethod(typeof(WalnutBushInjections), nameof(WalnutBushInjections.SetUpSourceRect_UseArchipelagoTexture_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Bush), nameof(Bush.draw), new[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(WalnutBushInjections), nameof(WalnutBushInjections.Draw_UseArchipelagoTexture_Prefix))
            );
        }

        private void PatchPuzzleWalnuts()
        {
            if (!_archipelago.SlotData.Walnutsanity.HasFlag(Walnutsanity.Puzzles))
            {
                return;
            }
            _harmony.Patch(
                original: AccessTools.Method(typeof(GeodeMenu), nameof(GeodeMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.ReceiveLeftClick_CrackGoldenCoconut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandEast), nameof(IslandEast.SpawnBananaNutReward)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.SpawnBananaNutReward_CheckInsteadOfNuts_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandHut), nameof(IslandHut.SpitTreeNut)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.SpitTreeNut_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandShrine), nameof(IslandShrine.OnPuzzleFinish)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.OnPuzzleFinish_CheckInsteadOfNuts_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandFarmCave), nameof(IslandFarmCave.GiveReward)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.GiveReward_GourmandCheckInsteadOfNuts_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(SandDuggy), nameof(SandDuggy.OnWhackedChanged)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.OnWhackedChanged_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandWestCave1), nameof(IslandWestCave1.UpdateWhenCurrentLocation)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.UpdateWhenCurrentLocation_CheckInsteadOfNuts_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandFieldOffice), "ApplyPlantRestoreLeft"),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.ApplyPlantRestoreLeft_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandFieldOffice), "ApplyPlantRestoreRight"),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.ApplyPlantRestoreRight_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandFieldOffice), nameof(IslandFieldOffice.donatePiece)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.DonatePiece_CheckInsteadOfNuts_Prefix))
            );
            var isCollidingPositionParameterTypes = new[]
            {
                typeof(Microsoft.Xna.Framework.Rectangle), typeof(xTile.Dimensions.Rectangle),
                typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool), typeof(bool)
            };
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandNorth), nameof(IslandNorth.isCollidingPosition), isCollidingPositionParameterTypes),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.IsCollidingPosition_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandSouthEast), nameof(IslandSouthEast.getFish)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.GetFish_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandSouthEast), nameof(IslandSouthEast.OnMermaidPuzzleSuccess)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.OnMermaidPuzzleSuccess_CheckInsteadOfNut_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Darts), nameof(Darts.QuitGame)),
                prefix: new HarmonyMethod(typeof(WalnutPuzzleInjections), nameof(WalnutPuzzleInjections.QuitGame_CheckInsteadOfNut_Prefix))
            );
        }

        private void PatchDigSpotWalnuts()
        {
            if (!_archipelago.SlotData.Walnutsanity.HasFlag(Walnutsanity.DigSpots))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandLocation), nameof(IslandLocation.checkForBuriedItem)),
                prefix: new HarmonyMethod(typeof(WalnutDigSpotsInjections), nameof(WalnutDigSpotsInjections.CheckForBuriedItem_ReplaceWalnutWithCheck_Prefix))
            );
        }

        private void PatchRepeatableWalnuts()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandLocation), nameof(IslandLocation.getFish)),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.GetFish_RepeatableWalnut_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.performUseAction)),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.PerformUseAction_RepeatableFarmingWalnut_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.performToolAction)),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.PerformToolAction_RepeatableFarmingWalnut_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "breakStone"),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.BreakStone_RepeatableMusselWalnut_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(VolcanoDungeon), "breakStone"),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.BreakStone_RepeatableVolcanoStoneWalnut_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(VolcanoDungeon), nameof(VolcanoDungeon.monsterDrop)),
                prefix: new HarmonyMethod(typeof(WalnutRepeatablesInjections), nameof(WalnutRepeatablesInjections.MonsterDrop_RepeatableVolcanoMonsterWalnut_Prefix))
            );
        }

        private void PatchScouts()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.exitThisMenu)),
                prefix: new HarmonyMethod(typeof(ScoutInjections), nameof(ScoutInjections.ExitThisMenu_ScoutShopContent_Prefix))
            );
        }

        private void PatchSecrets()
        {
            PatchEasySecrets();
            PatchFishableSecrets();
            PatchDifficultSecrets();
            PatchSecretNotesSecrets();
        }

        private void PatchEasySecrets()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), new[] { typeof(SpriteBatch)}),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.Draw_JungleJunimo_Postfix))
            );

            if (!_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.Easy))
            {
                return;
            }

            PatchPurpleShortsSecrets();

            _harmony.Patch(
                original: AccessTools.Method(typeof(TV), nameof(TV.proceedToNextScene)),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.ProceedToNextScene_ForsakenSouls_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.DayUpdate)),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.DayUpdate_SomethingForSanta_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MapPage), nameof(MapPage.receiveLeftClick)),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.ReceiveLeftClick_LonelyStone_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.ReceiveLeftClick_JungleJunimo_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new[] { typeof(string[]), typeof(Farmer), typeof(Location)}),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.PerformAction_SecretActions_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemByMenuIfNecessary)),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.AddItemByMenuIfNecessary_FarAwayStone_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.AwardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.AwardFestivalPrize_Meowmere_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.playElliottPiano)),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.PlayElliottPiano_FamiliarTune_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.TryGetDialogue), new[] { typeof(string) }),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.TryGetDialogue_MonstersInHouse_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CharacterCustomization), "optionButtonClick"),
                prefix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.OptionButtonClick_NameEasterEggs_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(ChatBox), nameof(ChatBox.addNiceTryEasterEggMessage)),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.AddNiceTryEasterEggMessage_NiceTry_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(ChatCommands.DefaultHandlers), nameof(ChatCommands.DefaultHandlers.ConcernedApe)),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.ConcernedApe_EnjoyNewLifeHere_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(ChatCommands.DefaultHandlers), nameof(ChatCommands.DefaultHandlers.Qi)),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.Qi_WhatdYouExpect_Postfix))
            );

            PatchGiftDialogueSecrets();
        }

        private void PatchPurpleShortsSecrets()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Event.DefaultCommands), nameof(Event.DefaultCommands.SwitchEvent)),
                postfix: new HarmonyMethod(typeof(PurpleShortsInjections), nameof(PurpleShortsInjections.SwitchEvent_PurpleShortsInSoup_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.interpretGrangeResults)),
                postfix: new HarmonyMethod(typeof(PurpleShortsInjections), nameof(PurpleShortsInjections.InterpretGrangeResults_Bribe_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
                prefix: new HarmonyMethod(typeof(PurpleShortsInjections), nameof(PurpleShortsInjections.TryToReceiveActiveObject_ConfrontMarnie_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.drawDialogue)),
                prefix: new HarmonyMethod(typeof(PurpleShortsInjections), nameof(PurpleShortsInjections.DrawDialogue_ShortsResponses_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
                prefix: new HarmonyMethod(typeof(PurpleShortsInjections), nameof(PurpleShortsInjections.CheckAction_ShortsReactions_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.getBobberStyle)),
                postfix: new HarmonyMethod(typeof(PurpleShortsInjections), nameof(PurpleShortsInjections.GetBobberStyle_ShortsBobber_Postfix))
            );
        }

        private void PatchFishableSecrets()
        {
            if (!_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.Fishing))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.doneHoldingFish)),
                postfix: new HarmonyMethod(typeof(FishableSecretsInjections), nameof(FishableSecretsInjections.DoneHoldingFish_FishableSecret_Postfix))
            );
        }

        private void PatchGiftDialogueSecrets()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.GetGiftReaction)),
                postfix: new HarmonyMethod(typeof(SimpleSecretsInjections), nameof(SimpleSecretsInjections.GetGiftReaction_SpecialDialogues_Postfix))
            );
        }

        private void PatchDifficultSecrets()
        {
            if (!_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.Difficult))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(ShippingMenu), nameof(ShippingMenu.receiveLeftClick)),
                postfix: new HarmonyMethod(typeof(DifficultSecretsInjections), nameof(DifficultSecretsInjections.ReceiveLeftClick_AnnoyMoonMan_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(TemporaryAnimatedSprite), new []{ typeof(string), typeof(Rectangle), typeof(Vector2), typeof(bool), typeof(float), typeof(Color) }),
                postfix: new HarmonyMethod(typeof(DifficultSecretsInjections), nameof(DifficultSecretsInjections.TemporaryAnimatedSpriteConstructor_StrangeSightingAndBigFoot_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(TemporaryAnimatedSprite), "PlaySound"),
                postfix: new HarmonyMethod(typeof(DifficultSecretsInjections), nameof(DifficultSecretsInjections.PlaySound_StrangeSightingAndBigFoot_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(SeaMonsterTemporarySprite), new []{typeof(float), typeof(int), typeof(int), typeof(Vector2)}),
                postfix: new HarmonyMethod(typeof(DifficultSecretsInjections), nameof(DifficultSecretsInjections.SeaMonsterTemporarySpriteConstructor_SeaMonsterSighting_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Summit), "showQiCheatingEvent"),
                postfix: new HarmonyMethod(typeof(DifficultSecretsInjections), nameof(DifficultSecretsInjections.ShowQiCheatingEvent_MeMeMeMeMeMeMeMeMeMeMeMeMeMeMeMe_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(HUDMessage), nameof(HUDMessage.numbersEasterEgg)),
                postfix: new HarmonyMethod(typeof(DifficultSecretsInjections), nameof(DifficultSecretsInjections.NumbersEasterEgg_StackMasterTrophy_Postfix))
            );
        }

        private void PatchSecretNotes()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.tryToCreateUnseenSecretNote)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.TryToCreateUnseenSecretNote_AllowSecretNotesIfStillNeedToShipThem_Postfix))
            );
        }

        private void PatchSecretNotesSecrets()
        { 
            if (!_archipelago.SlotData.Secretsanity.HasFlag(Secretsanity.SecretNotes))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.onGiftGiven)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.OnGiftGiven_GiftingNotes_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Constructor(typeof(LetterViewerMenu), new[] {typeof(int)}),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.LetterViewerMenuConstructor_ReadSecretNote_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.TryGetGarbageItem)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.TryGetGarbageItem_TrashCanSpecials_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Bush), nameof(Bush.junimoPlushCallback)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.JunimoPlushCallback_SendCheckAndRemovePlush_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.performToolAction)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.PerformToolAction_StoneJunimo_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MermaidHouse), nameof(MermaidHouse.playClamTone), new[] { typeof(int), typeof(Farmer) }),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.PlayClamTone_SongFinished_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Railroad), nameof(Railroad.checkForBuriedItem)),
                prefix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.CheckForBuriedItem_TreasureChest_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Town), nameof(Town.checkForBuriedItem)),
                prefix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.CheckForBuriedItem_GreenDoll_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Desert), nameof(Desert.checkForBuriedItem)),
                prefix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.CheckForBuriedItem_YellowDoll_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Town), nameof(Town.checkAction)),
                prefix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.CheckAction_FindGoldLewis_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.rot)),
                prefix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.Rot_GoldLewisFound_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.AnswerDialogueAction_SpecialCharmPurchase_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Town), nameof(Town.initiateMarnieLewisBush)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.InitiateMarnieLewisBush_BushShaken_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(JunimoHut), "getGemColor"),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.GetGemColor_HasColoredJunimos_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Railroad), nameof(Railroad.getFish)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.GetFish_CatchOrnateNecklace_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(JunimoHut), nameof(JunimoHut.dayUpdate)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.DayUpdate_HasRaisinFedJunimos_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.resetForPlayerEntry)),
                postfix: new HarmonyMethod(typeof(SecretNotesInjections), nameof(SecretNotesInjections.ResetForPlayerEntry_FoundCompendium_Postfix))
            );
        }

        private void PatchMoviesanity()
        {
            if (_archipelago.SlotData.Moviesanity <= Moviesanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(MovieTheater), nameof(MovieTheater.RequestEndMovie)),
                prefix: new HarmonyMethod(typeof(MovieInjections), nameof(MovieInjections.RequestEndMovie_SendMoviesanityLocations_Prefix)),
                postfix: new HarmonyMethod(typeof(MovieInjections), nameof(MovieInjections.RequestEndMovie_SendMoviesanityLocations_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MovieTheater), nameof(MovieTheater.GetMoviesForSeason)),
                prefix: new HarmonyMethod(typeof(MovieInjections), nameof(MovieInjections.GetMoviesForSeason_LoopEveryWeek_Prefix))
            );

            if (_archipelago.SlotData.Moviesanity <= Moviesanity.One)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(MovieTheater), nameof(MovieTheater.GetConcessionsForGuest), new[] { typeof(string) }),
                prefix: new HarmonyMethod(typeof(MovieInjections), nameof(MovieInjections.GetConcessionsForGuest_OfferAllUnlockedSnacks_Prefix))
            );
        }

        private void PatchHatsanity()
        {
            if (_archipelago.SlotData.Hatsanity <= Hatsanity.None)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Item), nameof(Item.onEquip)),
                postfix: new HarmonyMethod(typeof(HatInjections), nameof(HatInjections.OnEquip_EquippedHat_Postfix))
            );
        }

        private void PatchEating()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
                prefix: new HarmonyMethod(typeof(EatInjections), nameof(EatInjections.DoneEating_EatingPatches_Prefix))
            );

            if (!_archipelago.SlotData.Eatsanity.HasFlag(Eatsanity.LockEffects))
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.staminaRecoveredOnConsumption)),
                postfix: new HarmonyMethod(typeof(EatInjections), nameof(EatInjections.StaminaRecoveredOnConsumption_LimitToEnzymes_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.healthRecoveredOnConsumption)),
                postfix: new HarmonyMethod(typeof(EatInjections), nameof(EatInjections.HealthRecoveredOnConsumption_LimitToEnzymes_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.TryCreateBuffsFromData)),
                postfix: new HarmonyMethod(typeof(EatInjections), nameof(EatInjections.TryCreateBuffsFromData_LimitToEnzymes_Postfix))
            );
        }

        private void PatchEndgameLocations()
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new[] { typeof(string[]), typeof(Farmer), typeof(Location)}),
                prefix: new HarmonyMethod(typeof(CasinoInjections), nameof(CasinoInjections.PerformAction_OfferStatueOfEndlessFortune_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(CasinoInjections), nameof(CasinoInjections.AnswerDialogueAction_PurchaseStatueOfEndlessFortune_Prefix))
            );
        }

        private void PatchGarbageCans()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.TryGetGarbageItem)),
                postfix: new HarmonyMethod(typeof(GarbageInjections), nameof(GarbageInjections.TryGetGarbageItem_TagItemWithTrash_Postfix))
            );
        }
    }
}
