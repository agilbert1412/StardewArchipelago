using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Bundles;
using StardewArchipelago.GameModifications.CodeInjections.Television;
using StardewArchipelago.Goals;
using StardewArchipelago.Items.Traps;
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
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class VanillaCodeInjectionInitializer
    {
        public static void Initialize(LogHandler logger, IModHelper modHelper, ModConfig config, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, BundlesManager bundlesManager, Friends friends, TrapManager trapManager, BankHandler bank, NameSimplifier nameSimplifier, GiftSender giftSender, NightShippingBehaviors nightShippingBehaviors)
        {
            TrashBearInjections.Initialize(logger, modHelper, archipelago, locationChecker, bundlesManager, itemManager, state);
            BackpackInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            ScytheInjections.Initialize(logger, locationChecker);
            FishingRodInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CopperPanInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            var bundleReader = new BundleReader();
            var killList = new MonsterKillList(archipelago);
            GoalCodeInjection.Initialize(logger, modHelper, archipelago, locationChecker, bundleReader, killList);
            InitializeBundleInjections(logger, modHelper, archipelago, state, locationChecker, bundlesManager, bundleReader, trapManager, bank, giftSender);
            MineshaftInjections.Initialize(logger, modHelper, config, archipelago, locationChecker, jojaLocationChecker);
            InitializeSkills(logger, modHelper, archipelago, locationChecker);
            QuestInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            DarkTalismanInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CarpenterInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            IsolatedEventInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            WizardBookInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            PhoneInjections.Initialize(logger, modHelper, archipelago, weaponsManager);
            InitializeArcadeMachines(logger, modHelper, archipelago, locationChecker);
            TravelingMerchantInjections.Initialize(logger, modHelper, archipelago, locationChecker, state);
            FishingInjections.Initialize(logger, modHelper, archipelago, locationChecker, itemManager, state.Wallet);
            MuseumInjections.Initialize(logger, modHelper, archipelago, locationChecker, itemManager);
            FriendshipInjections.Initialize(logger, modHelper, archipelago, locationChecker, friends, itemManager);
            SpecialOrderInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            SpouseInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            PregnancyInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CropsanityInjections.Initialize(logger, archipelago, locationChecker, itemManager);
            InitializeFestivalPatches(logger, modHelper, archipelago, state.Wallet, locationChecker);
            MonsterSlayerInjections.Initialize(logger, modHelper, archipelago, locationChecker, killList);
            CookingInjections.Initialize(logger, archipelago, locationChecker, itemManager);
            var qosManager = new QueenOfSauceManager(state);
            QueenOfSauceInjections.Initialize(logger, modHelper, archipelago, locationChecker, itemManager, qosManager);
            RecipeLevelUpInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            ConversationFriendshipInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            RecipeFriendshipInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            CraftingInjections.Initialize(logger, modHelper, archipelago, itemManager, locationChecker);
            FarmCaveInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FarmEventInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            BookInjections.Initialize(logger, modHelper, archipelago, locationChecker, qosManager);
            InitializeWalnutsanityInjections(logger, modHelper, archipelago, locationChecker);
            EventInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            ScoutInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            InitializeSecretsInjections(logger, modHelper, archipelago, locationChecker);
            MovieInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            HatInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            EatInjections.Initialize(logger, modHelper, archipelago, locationChecker, nameSimplifier);
            CasinoInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            GarbageInjections.Initialize(logger, archipelago, locationChecker);
            CollectionsInjections.Initialize(logger, archipelago, locationChecker, itemManager, nightShippingBehaviors);
        }

        private static void InitializeArcadeMachines(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            JotPKInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            JunimoKartInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }

        private static void InitializeBundleInjections(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, StardewLocationChecker locationChecker, BundlesManager bundlesManager, BundleReader bundleReader, TrapManager trapManager, BankHandler bank, GiftSender giftSender)
        {
            ArchipelagoJunimoNoteMenu.InitializeArchipelago(logger, modHelper, archipelago, state, bank, locationChecker, trapManager);
            ArchipelagoBundle.InitializeArchipelago(logger, modHelper, archipelago, state, locationChecker, bundlesManager);
            CommunityCenterInjections.Initialize(logger, archipelago, locationChecker, bundleReader);
            RaccoonInjections.Initialize(logger, modHelper, archipelago, state, locationChecker, bundlesManager, bundleReader);
            WellInjections.Initialize(logger, modHelper, archipelago, locationChecker, giftSender);
            HorseInjections.Initialize(logger, modHelper, archipelago, locationChecker);
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

        private static void InitializeFestivalPatches(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoWalletDto wallet, LocationChecker locationChecker)
        {
            EggFestivalInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            DesertFestivalInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FlowerDanceInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            LuauInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            TroutDerbyInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            MoonlightJelliesInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FairInjections.Initialize(logger, modHelper, archipelago, wallet, locationChecker);
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

        private static void InitializeSecretsInjections(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            PurpleShortsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            SimpleSecretsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            FishableSecretsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            DifficultSecretsInjections.Initialize(logger, modHelper, archipelago, locationChecker);
            SecretNotesInjections.Initialize(logger, modHelper, archipelago, locationChecker);
        }
    }
}
