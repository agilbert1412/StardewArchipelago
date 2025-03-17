using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.GameModifications.CodeInjections.Bundles;
using StardewArchipelago.GameModifications.CodeInjections.Television;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.GameModifications.Tooltips;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Constants;
using StardewValley.Events;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using Object = StardewValley.Object;
using StardewArchipelago.Locations;
using StardewArchipelago.Logging;
using StardewValley.TerrainFeatures;

namespace StardewArchipelago.GameModifications
{
    public class RandomizedLogicPatcher
    {
        private readonly Harmony _harmony;
        private readonly IModHelper _helper;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;
        private readonly StartingResources _startingResources;
        private readonly SeedShopStockModifier _seedShopStockModifier;
        private readonly RecipeDataRemover _recipeDataRemover;
        private readonly AnimalShopStockModifier _animalShopStockModifier;
        private readonly JojaDisabler _jojaDisabler;

        public RandomizedLogicPatcher(LogHandler logger, IModHelper modHelper, ModConfig config, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, StardewItemManager stardewItemManager, EntranceManager entranceManager, SeedShopStockModifier seedShopStockModifier, NameSimplifier nameSimplifier, Friends friends, ArchipelagoStateDto state)
        {
            _harmony = harmony;
            _helper = modHelper;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _startingResources = new StartingResources(_archipelago, locationChecker, _stardewItemManager);
            _seedShopStockModifier = seedShopStockModifier;
            _recipeDataRemover = new RecipeDataRemover(logger, modHelper, archipelago);
            ArchipelagoLocationsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            MineshaftLogicInjections.Initialize(logger);
            CommunityCenterLogicInjections.Initialize(logger, locationChecker);
            FarmInjections.Initialize(logger, _archipelago);
            FarmerInjections.Initialize(logger, _archipelago);
            EventInjections.Initialize(logger, _archipelago);
            AchievementInjections.Initialize(logger, _archipelago);
            EntranceInjections.Initialize(logger, _helper, _archipelago, entranceManager);
            ForestInjections.Initialize(logger, _archipelago);
            MountainInjections.Initialize(logger, modHelper, _archipelago);
            TheaterInjections.Initialize(logger, modHelper, archipelago);
            LostAndFoundInjections.Initialize(logger, archipelago);
            InitializeTVInjections(logger, modHelper, archipelago, entranceManager, state);
            ProfitInjections.Initialize(logger, archipelago);
            QuestLogInjections.Initialize(logger, archipelago, locationChecker);
            WorldChangeEventInjections.Initialize(logger);
            CropInjections.Initialize(logger, archipelago, stardewItemManager);
            SecretNoteInjections.Initialize(logger, archipelago, locationChecker);
            KentInjections.Initialize(logger, archipelago);
            _animalShopStockModifier = new AnimalShopStockModifier(logger, modHelper, archipelago, stardewItemManager);
            GoldenClockInjections.Initialize(logger, archipelago);
            ZeldaAnimationInjections.Initialize(logger, archipelago);
            ItemTooltipInjections.Initialize(logger, modHelper, config, archipelago, locationChecker, nameSimplifier);
            BillboardInjections.Initialize(logger, modHelper, config, archipelago, locationChecker, friends);
            SpecialOrderBoardInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CraftingPageInjections.Initialize(logger, archipelago);
            PanningSpotInjections.Initialize(logger, archipelago);
            WalnutInjections.Initialize(logger, archipelago);
            OutOfLogicInjections.Initialize(logger, archipelago, stardewItemManager);
            EmptyHandInjections.Initialize(logger, archipelago, stardewItemManager);
            DebugPatchInjections.Initialize(logger, archipelago);
            _jojaDisabler = new JojaDisabler(logger, modHelper, harmony);
        }

        private static void InitializeTVInjections(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, EntranceManager entranceManager,
            ArchipelagoStateDto state)
        {
            TVInjections.Initialize(logger, archipelago);
            LivinOffTheLandInjections.Initialize(logger, archipelago);
            GatewayGazetteInjections.Initialize(logger, modHelper, archipelago, entranceManager, state);
        }

        public void PatchAllGameLogic()
        {
            PatchPickUpLocation();
            PatchAchievements();
            PatchMineMaxFloorReached();
            PatchDefinitionOfCommunityCenterComplete();
            PatchGrandpaNote();
            PatchDebris();
            PatchTown();
            PatchForest();
            PatchMountain();
            PatchEntrances();
            PatchSeasons();
            PatchSeedShops();
            PatchJodiFishQuest();
            PatchQuestLog();
            PatchWorldChangedEvent();
            PatchLostAndFoundBox();
            PatchMixedSeeds();
            PatchTvChannels();
            PatchCleanupBeforeSave();
            PatchProfitMargin();
            PatchKent();
            PatchGoldenEgg();
            PatchGoldenClock();
            PatchZeldaAnimations();
            MakeLegendaryFishAndVoidMayoRecatchable();
            PatchSecretNotes();
            PatchRecipes();
            PatchTooltips();
            PatchBundles();
            PatchCraftingPage();
            PatchPanningSpots();
            PatchWalnuts();
            PatchMysteryBoxesAndPrizeTickets();
            PatchStardropMessage();
            PatchLeoMove();
            PatchEmptyHandBreak();

            _jojaDisabler.DisableJojaRouteShortcuts();
            _startingResources.GivePlayerStartingResources();

            PatchDebugMethods();
        }

        public void CleanEvents()
        {
            CleanLegendaryFishAndVoidMayoRecatchableEvent();
            UnpatchSeedShops();
            UnpatchJodiFishQuest();
            CleanGoldenEggEvent();
        }

        private void PatchPickUpLocation()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.OnItemReceived)),
                prefix: new HarmonyMethod(typeof(ArchipelagoLocationsInjections), nameof(ArchipelagoLocationsInjections.OnItemReceived_PickUpACheck_Prefix))
            );

            var inventoryIdArguments = new[] { typeof(string), typeof(int), typeof(int) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), inventoryIdArguments),
                prefix: new HarmonyMethod(typeof(ArchipelagoLocationsInjections), nameof(ArchipelagoLocationsInjections.CouldInventoryAcceptThisItemById_ChecksFlyingAround_Prefix))
            );

            var inventoryItemArguments = new[] { typeof(Item) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.couldInventoryAcceptThisItem), inventoryItemArguments),
                prefix: new HarmonyMethod(typeof(ArchipelagoLocationsInjections), nameof(ArchipelagoLocationsInjections.CouldInventoryAcceptThisItemByItem_ChecksFlyingAround_Prefix))
            );
        }

        private void PatchAchievements()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.getSteamAchievement)),
                prefix: new HarmonyMethod(typeof(AchievementInjections), nameof(AchievementInjections.GetSteamAchievement_DisableUndeservedAchievements_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Stats), nameof(Stats.checkForMoneyAchievements)),
                prefix: new HarmonyMethod(typeof(AchievementInjections), nameof(AchievementInjections.CheckForMoneyAchievements_GrantMoneyAchievementsFairly_Prefix))
            );
        }

        private void PatchMineMaxFloorReached()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(NetWorldState), typeof(NetWorldState).GetProperty(nameof(NetWorldState.LowestMineLevel)).GetSetMethod().Name),
                prefix: new HarmonyMethod(typeof(MineshaftLogicInjections), nameof(MineshaftLogicInjections.SetLowestMineLevel_SkipToSkullCavern_Prefix))
            );
        }

        private void PatchDefinitionOfCommunityCenterComplete()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.hasCompletedCommunityCenter)),
                prefix: new HarmonyMethod(typeof(CommunityCenterLogicInjections), nameof(CommunityCenterLogicInjections.HasCompletedCommunityCenter_CheckGameStateInsteadOfLetters_Prefix))
            );

            var town = Game1.getLocationFromName("Town");
            town.TryGetLocationEvents(out var assetName, out var townEvents);
            var communityCenterCeremonyEventKey = "";
            var communityCenterCeremonyEventValue = "";

            foreach (var (key, value) in townEvents)
            {
                if (!key.StartsWith("191393"))
                {
                    continue;
                }

                communityCenterCeremonyEventKey = key;
                communityCenterCeremonyEventValue = value;
            }
            townEvents.Remove(communityCenterCeremonyEventKey);
            communityCenterCeremonyEventKey = communityCenterCeremonyEventKey.Replace(" cc", " apcc");
            townEvents.Add(communityCenterCeremonyEventKey, communityCenterCeremonyEventValue);

            SendMissedAPCCMails();
        }

        private static void SendMissedAPCCMails()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            if (!communityCenter.areAllAreasComplete())
            {
                return;
            }
            string[] apccMails =
                { "apccPantry", "apccCraftsRoom", "apccFishTank", "apccBoilerRoom", "apccVault", "apccBulletin" };
            foreach (var apccMail in apccMails)
            {
                if (!Game1.player.mailReceived.Contains(apccMail))
                {
                    Game1.player.mailForTomorrow.Add(apccMail + "%&NL&%");
                }
            }
        }

        private void PatchGrandpaNote()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farm), nameof(Farm.checkAction)),
                prefix: new HarmonyMethod(typeof(FarmInjections), nameof(FarmInjections.CheckAction_GrandpaNote_PreFix))
            );
        }

        private void PatchDebris()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnWeedsAndStones)),
                prefix: new HarmonyMethod(typeof(FarmInjections), nameof(FarmInjections.SpawnWeedsAndStones_ConsiderUserPreference_PreFix))
            );
        }

        private void PatchTown()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.MakeMapModifications)),
                prefix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.MakeMapModifications_PlaceMissingBundleNote_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkTileIndexAction)),
                prefix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.CheckTileIndexAction_InteractWithMissingBundleNote_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbandonedJojaMart), "doRestoreAreaCutscene"),
                prefix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.DoRestoreAreaCutscene_InteractWithMissingBundleNote_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Town), nameof(Town.MakeMapModifications)),
                prefix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.MakeMapModifications_JojamartAndTheater_Prefix)),
                postfix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.MakeMapModifications_JojamartAndTheater_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(NPC), "changeScheduleForLocationAccessibility"),
                prefix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.ChangeScheduleForLocationAccessibility_JojamartAndTheater_Prefix))
            );

            //_harmony.Patch(
            //    original: AccessTools.Method(typeof(NPC), nameof(NPC.parseMasterSchedule)),
            //    prefix: new HarmonyMethod(typeof(TheaterInjections), nameof(TheaterInjections.ParseMasterSchedule_JojamartAndTheater_Prefix))
            //);
        }

        private void PatchForest()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), "isWizardHouseUnlocked"),
                prefix: new HarmonyMethod(typeof(ForestInjections), nameof(ForestInjections.IsWizardHouseUnlocked_UnlockAtRatProblem_Prefix))
            );
        }

        private void PatchMountain()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Mountain), nameof(Mountain.ApplyTreehouseIfNecessary)),
                prefix: new HarmonyMethod(typeof(MountainInjections), nameof(MountainInjections.ApplyTreehouseIfNecessary_ApplyTreeHouseIfReceivedApItem_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Mountain), nameof(Mountain.DayUpdate)),
                postfix: new HarmonyMethod(typeof(MountainInjections), nameof(MountainInjections.DayUpdate_RailroadDependsOnApItem_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Mountain), "resetSharedState"),
                postfix: new HarmonyMethod(typeof(MountainInjections), nameof(MountainInjections.ResetSharedState_RailroadDependsOnApItem_Postfix))
            );

            MountainInjections.SetRailroadBlockedBasedOnArchipelagoItem((Mountain)Game1.getLocationFromName("Mountain"));
        }

        private void PatchEntrances()
        {
            if (_archipelago.SlotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.warpFarmer), new[] { typeof(string), typeof(int), typeof(int), typeof(int) }),
                prefix: new HarmonyMethod(typeof(EntranceInjections), nameof(EntranceInjections.WarpFarmer_InterceptProblemEntrances_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "performWarpFarmer"),
                prefix: new HarmonyMethod(typeof(EntranceInjections), nameof(EntranceInjections.PerformWarpFarmer_EntranceRandomization_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                postfix: new HarmonyMethod(typeof(EntranceInjections), nameof(EntranceInjections.PlacementAction_DontGlowShortsMaze_Postfix))
            );
        }

        private void PatchSeasons()
        {
            // Game1: public static void loadForNewGame(bool loadedGame = false)
            // Game1: private static void newSeason()
            // Game1: public static void NewDay(float timeToPause)

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.NewDay)),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.NewDay_SeasonChoice_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "OnNewSeason"),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.OnNewSeason_UsePredefinedChoice_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.AnswerDialogueAction_SeasonChoice_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.Date)),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.Date_UseTotalDaysStats_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.PropertyGetter(typeof(Friendship), nameof(Friendship.CountdownToWedding)),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.CountdownToWedding_Add1_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.getWeatherModificationsForDate)),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.GetWeatherModificationsForDate_UseCorrectDates_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(StatKeys), nameof(StatKeys.SquidFestScore)),
                prefix: new HarmonyMethod(typeof(SquidFestInjections), nameof(SquidFestInjections.SquidFestScore_UseMonthInsteadOfYear_Prefix))
            );
        }

        private void PatchSeedShops()
        {
            _helper.Events.Content.AssetRequested += _seedShopStockModifier.OnSeedShopStockRequested;
        }

        private void UnpatchSeedShops()
        {
            _helper.Events.Content.AssetRequested -= _seedShopStockModifier.OnSeedShopStockRequested;
        }

        private const string FISH_CASSEROLE_QUEST_ID = "22";

        private void PatchJodiFishQuest()
        {
            _helper.Events.Content.AssetRequested += FishCasseroleEventRequested;
        }

        private void UnpatchJodiFishQuest()
        {
            _helper.Events.Content.AssetRequested -= FishCasseroleEventRequested;
        }

        private void FishCasseroleEventRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            var isSamHouse = e.NameWithoutLocale.IsEquivalentTo("Data/Events/SamHouse");
            var isQuests = e.NameWithoutLocale.IsEquivalentTo("Data/Quests");

            if (!isSamHouse && !isQuests)
            {
                return;
            }

            if (isSamHouse)
            {
                e.Edit(ChangeFishCasseroleEventTriggerTimes, AssetEditPriority.Late);
            }

            if (isQuests)
            {
                e.Edit(ChangeFishCasseroleQuestText, AssetEditPriority.Late);
            }

            RefreshFishCasseroleQuestIfAlreadyHasIt();
        }

        private static void ChangeFishCasseroleEventTriggerTimes(IAssetData asset)
        {
            var jodiHouseEvents = asset.AsDictionary<string, string>().Data;
            const string originalTimeRequired = "1800 1950";
            const string newTimeRequired = "1200 2250";
            var eventsToFix = new HashSet<string>();

            foreach (var (key, value) in jodiHouseEvents)
            {
                if (!key.Contains(originalTimeRequired))
                {
                    continue;
                }

                eventsToFix.Add(key);
            }

            foreach (var eventToFix in eventsToFix)
            {
                var eventValue = jodiHouseEvents[eventToFix];
                jodiHouseEvents.Remove(eventToFix);
                var fixedKey = eventToFix.Replace(originalTimeRequired, newTimeRequired);
                jodiHouseEvents.Add(fixedKey, eventValue);
            }
        }

        private static void ChangeFishCasseroleQuestText(IAssetData asset)
        {
            var quests = asset.AsDictionary<string, string>().Data;
            var fishQuest = quests[FISH_CASSEROLE_QUEST_ID];
            var modifiedFishQuest = fishQuest.Replace("dinner at 7:00 PM.", "dinner.")
                .Replace("bass at 7:00 PM.", "bass in the afternoon.");
            quests[FISH_CASSEROLE_QUEST_ID] = modifiedFishQuest;
        }

        private static void RefreshFishCasseroleQuestIfAlreadyHasIt()
        {
            foreach (var quest in Game1.player.questLog.ToArray())
            {
                if (quest.id.Value != FISH_CASSEROLE_QUEST_ID || quest.completed.Value ||
                    !quest.currentObjective.Contains("7:00 PM"))
                {
                    continue;
                }

                Game1.player.removeQuest(FISH_CASSEROLE_QUEST_ID);
                Game1.player.addQuest(FISH_CASSEROLE_QUEST_ID);
            }
        }

        private void PatchLostAndFoundBox()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(FarmerTeam), nameof(FarmerTeam.CheckReturnedDonations)),
                prefix: new HarmonyMethod(typeof(LostAndFoundInjections), nameof(LostAndFoundInjections.CheckReturnedDonations_UpgradeToolsProperly_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.fixProblems)),
                postfix: new HarmonyMethod(typeof(LostAndFoundInjections), nameof(LostAndFoundInjections.FixProblems_DontLostAndFoundUnreceivedTools_Postfix))
            );
        }

        private void PatchQuestLog()
        {
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(QuestLog)),
                postfix: new HarmonyMethod(typeof(QuestLogInjections), nameof(QuestLogInjections.Constructor_MakeQuestsNonCancellable_Postfix))
            );

            if (!_archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.foundArtifact)),
                postfix: new HarmonyMethod(typeof(QuestLogInjections), nameof(QuestLogInjections.FoundArtifact_StartArchaeologyIfMissed_Postfix))
            );
        }

        private void PatchWorldChangedEvent()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(WorldChangeEvent), nameof(WorldChangeEvent.setUp)),
                prefix: new HarmonyMethod(typeof(WorldChangeEventInjections), nameof(WorldChangeEventInjections.SetUp_MakeSureEventsAreNotDuplicated_Prefix))
            );
        }

        private void PatchMixedSeeds()
        {
            if (_archipelago.SlotData.Cropsanity == Cropsanity.Disabled || !ModEntry.Instance.Config.StrictLogic)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.ResolveSeedId)),
                prefix: new HarmonyMethod(typeof(CropInjections), nameof(CropInjections.ResolveSeedId_MixedSeedsBecomesUnlockedCrop_Prefix))
            );
        }

        private void PatchTvChannels()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(TV), nameof(TV.checkForAction)),
                prefix: new HarmonyMethod(typeof(TVInjections), nameof(TVInjections.CheckForAction_TVChannels_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(TV), "getTodaysTip"),
                prefix: new HarmonyMethod(typeof(LivinOffTheLandInjections), nameof(LivinOffTheLandInjections.GetTodaysTip_CustomLivinOffTheLand_Prefix))
            );

            if (_archipelago.SlotData.EntranceRandomization is EntranceRandomization.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(TV), nameof(TV.selectChannel)),
                postfix: new HarmonyMethod(typeof(GatewayGazetteInjections), nameof(GatewayGazetteInjections.SelectChannel_SelectGatewayGazetteChannel_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(TV), nameof(TV.proceedToNextScene)),
                postfix: new HarmonyMethod(typeof(GatewayGazetteInjections), nameof(GatewayGazetteInjections.ProceedToNextScene_GatewayGazette_Postfix))
            );
        }

        private void PatchCleanupBeforeSave()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.cleanupBeforeSave)),
                postfix: new HarmonyMethod(typeof(CleanupBeforeSaveInjections), nameof(CleanupBeforeSaveInjections.CleanupBeforeSave_RemoveIllegalMonsters_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), nameof(CommunityCenter.cleanupBeforeSave)),
                postfix: new HarmonyMethod(typeof(CleanupBeforeSaveInjections), nameof(CleanupBeforeSaveInjections.CleanupBeforeSave_RemoveIllegalMonsters_Postfix))
            );
        }

        private void PatchProfitMargin()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
                postfix: new HarmonyMethod(typeof(ProfitInjections), nameof(ProfitInjections.SellToStorePrice_ApplyProfitMargin_Postfix))
            );
        }

        private void PatchKent()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.AddCharacterIfNecessary)),
                prefix: new HarmonyMethod(typeof(KentInjections), nameof(KentInjections.AddCharacterIfNecessary_ConsiderSeasonsRandomizerForKent_Prefix))
            );
        }

        private void PatchGoldenEgg()
        {
            _helper.Events.Content.AssetRequested += _animalShopStockModifier.OnShopStockRequested;
        }

        private void CleanGoldenEggEvent()
        {
            _helper.Events.Content.AssetRequested -= _animalShopStockModifier.OnShopStockRequested;
        }

        private void PatchGoldenClock()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Building), nameof(Building.doAction)),
                prefix: new HarmonyMethod(typeof(GoldenClockInjections), nameof(GoldenClockInjections.DoAction_GoldenClockIncreaseTime_Prefix))
            );
        }

        private void PatchZeldaAnimations()
        {
            var holdUpItemParameterTypes = new[] { typeof(Item), typeof(int), typeof(bool) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.holdUpItemThenMessage),
                    holdUpItemParameterTypes),
                prefix: new HarmonyMethod(typeof(ZeldaAnimationInjections),
                    nameof(ZeldaAnimationInjections.HoldUpItemThenMessage_SkipBasedOnConfig_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventory),
                    new[] { typeof(Item), typeof(List<Item>) }),
                postfix: new HarmonyMethod(typeof(ZeldaAnimationInjections),
                    nameof(ZeldaAnimationInjections.AddItemToInventory_AffectedItems_PrankDay_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventory),
                    new[] { typeof(Item), typeof(int) }),
                postfix: new HarmonyMethod(typeof(ZeldaAnimationInjections),
                    nameof(ZeldaAnimationInjections.AddItemToInventory_Position_PrankDay_Postfix))
            );
        }

        private void MakeLegendaryFishAndVoidMayoRecatchable()
        {
            _helper.Events.Content.AssetRequested += OnFishAssetRequested;
            _helper.GameContent.InvalidateCache("Data/Locations");
        }

        private void CleanLegendaryFishAndVoidMayoRecatchableEvent()
        {
            _helper.Events.Content.AssetRequested -= OnFishAssetRequested;
        }

        private void OnFishAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Locations"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    const string henchmanCondition = "!PLAYER_HAS_MAIL Host henchmanGone";
                    var locationsData = asset.AsDictionary<string, LocationData>().Data;

                    foreach (var (locationName, locationData) in locationsData)
                    {
                        foreach (var spawnFishData in locationData.Fish)
                        {
                            spawnFishData.CatchLimit = -1;

                            if (spawnFishData.Condition == null || !spawnFishData.Condition.Contains(henchmanCondition, StringComparison.InvariantCultureIgnoreCase))
                            {
                                continue;
                            }

                            var parts = spawnFishData.Condition.Split(',').ToList();
                            parts = parts.Where(x => !x.Contains(henchmanCondition, StringComparison.InvariantCultureIgnoreCase)).ToList();
                            spawnFishData.Condition = string.Join(',', parts);
                        }
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void PatchSecretNotes()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.tryToCreateUnseenSecretNote)),
                postfix: new HarmonyMethod(typeof(SecretNoteInjections), nameof(SecretNoteInjections.TryToCreateUnseenSecretNote_AllowSecretNotesIfStillNeedToShipThem_Postfix))
            );
        }

        private void PatchRecipes()
        {
            _helper.Events.Content.AssetRequested += _recipeDataRemover.OnCookingRecipesRequested;
            _helper.Events.Content.AssetRequested += _recipeDataRemover.OnCraftingRecipesRequested;

            _helper.GameContent.InvalidateCache("Data/CookingRecipes");
            _helper.GameContent.InvalidateCache("Data/CraftingRecipes");
        }

        private void UnPatchRecipes()
        {
            _helper.Events.Content.AssetRequested -= _recipeDataRemover.OnCookingRecipesRequested;
            _helper.Events.Content.AssetRequested -= _recipeDataRemover.OnCraftingRecipesRequested;
        }

        private void PatchTooltips()
        {
            var objectDrawParameters = new[]
                { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.drawInMenu), objectDrawParameters),
                postfix: new HarmonyMethod(typeof(ItemTooltipInjections), nameof(ItemTooltipInjections.DrawInMenu_AddArchipelagoLogoIfNeeded_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(ColoredObject), nameof(ColoredObject.drawInMenu), objectDrawParameters),
                postfix: new HarmonyMethod(typeof(ItemTooltipInjections), nameof(ItemTooltipInjections.DrawInMenuColored_AddArchipelagoLogoIfNeeded_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.getDescription)),
                postfix: new HarmonyMethod(typeof(ItemTooltipInjections), nameof(ItemTooltipInjections.GetDescription_AddMissingChecks_Postfix))
            );

            var boardDrawParameters = new[] { typeof(SpriteBatch) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(Billboard), nameof(Billboard.draw), boardDrawParameters),
                postfix: new HarmonyMethod(typeof(BillboardInjections), nameof(BillboardInjections.Draw_AddArchipelagoIndicators_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Billboard), nameof(Billboard.performHoverAction)),
                postfix: new HarmonyMethod(typeof(BillboardInjections), nameof(BillboardInjections.PerformHoverAction_AddArchipelagoChecksToTooltips_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(SpecialOrdersBoard), nameof(SpecialOrdersBoard.draw), boardDrawParameters),
                postfix: new HarmonyMethod(typeof(SpecialOrderBoardInjections), nameof(SpecialOrderBoardInjections.Draw_AddArchipelagoIndicators_Postfix))
            );
        }

        private void PatchBundles()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(JunimoNoteMenu), "setUpBundleSpecificPage"),
                transpiler: new HarmonyMethod(typeof(BundleMenuInjection), nameof(BundleMenuInjection.SkipObjectCheck))
            );
        }

        private void PatchCraftingPage()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.snapToDefaultClickableComponent)),
                prefix: new HarmonyMethod(typeof(CraftingPageInjections), nameof(CraftingPageInjections.SnapToDefaultClickableComponent_DontCrashIfEmpty_Prefix))
            );
        }

        private void PatchPanningSpots()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performOrePanTenMinuteUpdate)),
                prefix: new HarmonyMethod(typeof(PanningSpotInjections), nameof(PanningSpotInjections.PerformOrePanTenMinuteUpdate_AllowPanningSpotsAlways_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(IslandNorth), nameof(IslandNorth.performOrePanTenMinuteUpdate)),
                prefix: new HarmonyMethod(typeof(PanningSpotInjections), nameof(PanningSpotInjections.PerformOrePanTenMinuteUpdateOnIslandNorth_AllowPanningSpotsAlways_Prefix))
            );
        }

        private void PatchWalnuts()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.foundWalnut)),
                prefix: new HarmonyMethod(typeof(WalnutInjections), nameof(WalnutInjections.FoundWalnut_NoUpperLimit_Prefix))
            );
        }

        private void PatchMysteryBoxesAndPrizeTickets()
        {
            if (!ModEntry.Instance.Config.StrictLogic)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(PrizeTicketMenu), nameof(PrizeTicketMenu.getPrizeItem)),
                postfix: new HarmonyMethod(typeof(OutOfLogicInjections), nameof(OutOfLogicInjections.GetPrizeItem_SkipOutOfLogicPrizeTickets_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getTreasureFromGeode)),
                postfix: new HarmonyMethod(typeof(OutOfLogicInjections), nameof(OutOfLogicInjections.GetTreasureFromGeode_MysteryBoxesGiveReceivedItems_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getRaccoonSeedForCurrentTimeOfYear)),
                postfix: new HarmonyMethod(typeof(OutOfLogicInjections), nameof(OutOfLogicInjections.GetRaccoonSeedForCurrentTimeOfYear_MysteryBoxesGiveReceivedItems_Postfix))
            );
        }

        private void PatchStardropMessage()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
                postfix: new HarmonyMethod(typeof(FarmerInjections), nameof(FarmerInjections.DoneEating_StardropFavoriteThingKaito_Postfix))
            );
        }

        private void PatchLeoMove()
        {
            if (_archipelago.SlotData.ExcludeGingerIsland)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.endBehaviors), new[] { typeof(string[]), typeof(GameLocation) }),
                prefix: new HarmonyMethod(typeof(EventInjections), nameof(EventInjections.EndBehaviors_LeoMoving_Prefix))
            );
        }

        private void PatchEmptyHandBreak()
        {
            if (!ModEntry.Instance.Config.AllowHandBreaking)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
                prefix: new HarmonyMethod(typeof(EmptyHandInjections), nameof(EmptyHandInjections.CheckAction_BreakSomethingByHand_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.performUseAction)),
                prefix: new HarmonyMethod(typeof(EmptyHandInjections), nameof(EmptyHandInjections.PerformUseAction_BreakTreeByHand_Prefix))
            );
        }

        private void PatchDebugMethods()
        {

        }
    }
}
