using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.GameModifications.Seasons;
using StardewArchipelago.Locations;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewArchipelago.GameModifications
{
    public class RandomizedLogicPatcher
    {
        private readonly Harmony _harmony;
        private readonly ArchipelagoClient _archipelago;
        private readonly StardewItemManager _stardewItemManager;
        private readonly StartingResources _startingResources;

        public RandomizedLogicPatcher(IMonitor monitor, IModHelper helper, Harmony harmony, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager stardewItemManager, ArchipelagoStateDto state)
        {
            _harmony = harmony;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _startingResources = new StartingResources(_archipelago, _stardewItemManager);
            MineshaftLogicInjections.Initialize(monitor);
            CommunityCenterLogicInjections.Initialize(monitor, locationChecker);
            FarmInjections.Initialize(monitor, _archipelago);
            AchievementInjections.Initialize(monitor, _archipelago);
            EntranceInjections.Initialize(monitor, _archipelago);
            ForestInjections.Initialize(monitor, _archipelago);
            SeedShopsInjections.Initialize(monitor, helper, archipelago, locationChecker);
        }

        public void PatchAllGameLogic()
        {
            PatchAchievements();
            PatchMineMaxFloorReached();
            PatchDefinitionOfCommunityCenterComplete();
            PatchGrandpaNote();
            PatchDebris();
            PatchForest();
            PatchEntrances();
            PatchSeasons();
            PatchSeedShops();
            // PatchAppearanceRandomization();
            _startingResources.GivePlayerStartingResources();
        }

        private void PatchAchievements()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.getSteamAchievement)),
                prefix: new HarmonyMethod(typeof(AchievementInjections), nameof(AchievementInjections.GetSteamAchievement_DisableUndeservedAchievements_Prefix))
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

            var townEvents = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\Town");
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

        private void PatchForest()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Forest), "isWizardHouseUnlocked"),
                prefix: new HarmonyMethod(typeof(ForestInjections), nameof(ForestInjections.IsWizardHouseUnlocked_UnlockAtRatProblem_Prefix))
            );
        }

        private void PatchEntrances()
        {
            if (_archipelago.SlotData.EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "performWarpFarmer"),
                prefix: new HarmonyMethod(typeof(EntranceInjections), nameof(EntranceInjections.PerformWarpFarmer_EntranceRandomization_Prefix))
            );
        }

        private void PatchSeasons()
        {
            // Game1: public static void loadForNewGame(bool loadedGame = false)
            // Game1: private static void newSeason()
            // Game1: public static void NewDay(float timeToPause)

            var original = AccessTools.Method(typeof(Game1), nameof(Game1.NewDay));
            var prefix = new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.NewDay_SeasonChoice_Prefix));

            _harmony.Patch(
                original: original,
                prefix: prefix
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), "newSeason"),
                prefix: new HarmonyMethod(typeof(SeasonsRandomizer), nameof(SeasonsRandomizer.NewSeason_UsePredefinedChoice_Prefix))
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

            SeasonsRandomizer.ChangeMailKeysBasedOnSeasonsToDaysElapsed();
        }

        private void PatchSeedShops()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.openShopMenu)),
                prefix: new HarmonyMethod(typeof(SeedShopsInjections), nameof(SeedShopsInjections.OpenShopMenu_PierrePersistentEvent_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getJojaStock)),
                prefix: new HarmonyMethod(typeof(SeedShopsInjections), nameof(SeedShopsInjections.GetJojaStock_FullCostco_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), "sandyShopStock"),
                prefix: new HarmonyMethod(typeof(SeedShopsInjections), nameof(SeedShopsInjections.SandyShopStock_LimitedStock_Prefix))
            );

            var shopMenuParameterTypes = new[]
            {
                typeof(Dictionary<ISalable, int[]>), typeof(int), typeof(string),
                typeof(Func<ISalable, Farmer, int, bool>), typeof(Func<ISalable, bool>), typeof(string)
            };
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(ShopMenu), shopMenuParameterTypes),
                prefix: new HarmonyMethod(typeof(SeedShopsInjections), nameof(SeedShopsInjections.ShopMenu_SeedShuffle_Prefix))
            ); 
        }

        private void PatchAppearanceRandomization()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(AnimatedSprite), nameof(AnimatedSprite.LoadTexture)),
                prefix: new HarmonyMethod(typeof(AppearanceRandomizer), nameof(AppearanceRandomizer.LoadTexture_ShuffleAppearance_Prefix))
            );
        }
    }
}
