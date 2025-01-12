using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications.CodeInjections.Television;
using StardewArchipelago.Goals;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Arcade;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Quests;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity;
using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class VanillaCodeInjectionInitializer
    {
        public static void Initialize(LogHandler logger, IModHelper modHelper, ModConfig config, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, StardewLocationChecker locationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, BundlesManager bundlesManager, Friends friends)
        {
            BackpackInjections.Initialize(logger, archipelago, locationChecker);
            ScytheInjections.Initialize(logger, locationChecker);
            FishingRodInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CopperPanInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            var bundleReader = new BundleReader();
            var killList = new MonsterKillList(archipelago);
            GoalCodeInjection.Initialize(logger, modHelper, archipelago, locationChecker, bundleReader, killList);
            InitializeBundleInjections(logger, modHelper, archipelago, state, locationChecker, bundlesManager, bundleReader);
            MineshaftInjections.Initialize(logger, modHelper, config, archipelago, locationChecker);
            InitializeSkills(logger, modHelper, archipelago, locationChecker);
            QuestInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            DarkTalismanInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CarpenterInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IsolatedEventInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            WizardBookInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            PhoneInjections.Initialize(logger, modHelper, archipelago, weaponsManager);
            InitializeArcadeMachines(logger, modHelper, archipelago, locationChecker);
            TravelingMerchantInjections.Initialize(logger, modHelper, archipelago, locationChecker, state);
            FishingInjections.Initialize(logger, modHelper, archipelago, locationChecker, itemManager);
            MuseumInjections.Initialize(logger, modHelper, archipelago, locationChecker, itemManager);
            FriendshipInjections.Initialize(logger, modHelper, archipelago, locationChecker, friends, itemManager);
            SpecialOrderInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            SpouseInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            PregnancyInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CropsanityInjections.Initialize(logger, archipelago, locationChecker, itemManager);
            InitializeFestivalPatches(logger, modHelper, archipelago, state, locationChecker);
            MonsterSlayerInjections.Initialize(logger, modHelper, archipelago, locationChecker, killList);
            CookingInjections.Initialize(logger, archipelago, locationChecker, itemManager);
            var qosManager = new QueenOfSauceManager(state);
            QueenOfSauceInjections.Initialize(logger, modHelper, archipelago, locationChecker, itemManager, qosManager);
            RecipeLevelUpInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            RecipeFriendshipInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CraftingInjections.Initialize(logger, modHelper, archipelago, itemManager, locationChecker);
            FarmCaveInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FarmEventInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            BookInjections.Initialize(logger, modHelper, archipelago, locationChecker, qosManager);
            InitializeWalnutsanityInjections(logger, modHelper, archipelago, locationChecker);
            EventInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            InitializeSecretsInjections(logger, modHelper, archipelago, locationChecker);
        }

        private static void InitializeArcadeMachines(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            JotPKInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            JunimoKartInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }

        private static void InitializeBundleInjections(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundlesManager bundlesManager, BundleReader bundleReader)
        {
            CommunityCenterInjections.Initialize(logger, archipelago, locationChecker, bundleReader);
            JunimoNoteMenuInjections.Initialize(logger, modHelper, archipelago, state, locationChecker, bundleReader);
            BundleInjections.Initialize(logger, modHelper, archipelago, state, locationChecker, bundlesManager);
            RaccoonInjections.Initialize(logger, modHelper, archipelago, state, locationChecker, bundlesManager);
        }

        private static void InitializeSkills(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            SkillInjections.Initialize(logger, modHelper, archipelago, locationChecker);

            if (archipelago.SlotData.SkillProgression != SkillsProgression.ProgressiveWithMasteries)
            {
                return;
            }

            MasteriesInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }

        private static void InitializeFestivalPatches(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker)
        {
            EggFestivalInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            DesertFestivalInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FlowerDanceInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            LuauInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            TroutDerbyInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            MoonlightJelliesInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FairInjections.Initialize(logger, modHelper, archipelago, state, locationChecker);
            SpiritEveInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IceFestivalInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            SquidFestInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            MermaidHouseInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            BeachNightMarketInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            WinterStarInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }

        private static void InitializeWalnutsanityInjections(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            WalnutPuzzleInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            WalnutBushInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            WalnutDigSpotsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            WalnutRepeatablesInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }

        private static void InitializeSecretsInjections(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            PurpleShortsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            SimpleSecretsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FishableSecretsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            DifficultSecretsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }
    }
}
