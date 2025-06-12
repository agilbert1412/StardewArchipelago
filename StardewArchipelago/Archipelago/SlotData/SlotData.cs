using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.Testing;
using StardewArchipelago.Locations.Jojapocalypse.Consequences;

namespace StardewArchipelago.Archipelago.SlotData
{
    public class SlotData : ISlotData
    {
        private readonly Dictionary<string, object> _slotDataFields;
        private readonly ILogger _logger;

        public string SlotName { get; private set; }
        public Goal Goal { get; private set; }
        public FarmType FarmType { get; private set; }
        public int StartingMoney { get; private set; }
        public double ProfitMargin { get; private set; }
        public string BundlesData { get; set; }
        public BundlePrice BundlePrice { get; private set; }
        public EntranceRandomization EntranceRandomization { get; private set; }
        public SeasonRandomization SeasonRandomization { get; private set; }
        public Cropsanity Cropsanity { get; private set; }
        public BackpackProgression BackpackProgression { get; private set; }
        public int BackpackSize { get; private set; }
        public ToolProgression ToolProgression { get; private set; }
        public ElevatorProgression ElevatorProgression { get; private set; }
        public SkillsProgression SkillProgression { get; private set; }
        public BuildingProgression BuildingProgression { get; private set; }
        public FestivalLocations FestivalLocations { get; private set; }
        public ArcadeLocations ArcadeMachineLocations { get; private set; }
        public SpecialOrderLocations SpecialOrderLocations { get; private set; }
        public QuestLocations QuestLocations { get; private set; }
        public Fishsanity Fishsanity { get; private set; }
        public Museumsanity Museumsanity { get; private set; }
        public Monstersanity Monstersanity { get; private set; }
        public Shipsanity Shipsanity { get; private set; }
        public Cooksanity Cooksanity { get; private set; }
        public Chefsanity Chefsanity { get; private set; }
        public Craftsanity Craftsanity { get; private set; }
        public Friendsanity Friendsanity { get; private set; }
        public int FriendsanityHeartSize { get; private set; }
        public Eatsanity Eatsanity { get; private set; }
        public Booksanity Booksanity { get; private set; }
        public Walnutsanity Walnutsanity { get; private set; }
        public Moviesanity Moviesanity { get; private set; }
        public Secretsanity Secretsanity { get; private set; }
        public Hatsanity Hatsanity { get; set; }
        public bool IncludeEndgameLocations { get; set; }
        public bool ExcludeGingerIsland { get; private set; }
        public TrapItemsDifficulty TrapItemsDifficulty { get; set; }
        public bool EnableMultiSleep { get; private set; }
        public int MultiSleepCostPerDay { get; private set; }
        public double ExperienceMultiplier { get; private set; }
        public double FriendshipMultiplier { get; private set; }
        public DebrisMultiplier DebrisMultiplier { get; private set; }
        public bool QuickStart { get; private set; }
        public bool Gifting { get; private set; }
        public bool Banking { get; private set; }
        public bool DeathLink { get; private set; }
        public string Seed { get; private set; }
        public string MultiworldVersion { get; private set; }
        public Dictionary<string, string> ModifiedEntrances { get; set; }
        public ModsManager Mods { get; set; }
        public JojapocalypseSlotData Jojapocalypse { get; set; }

#if TILESANITY
        public Tilesanity Tilesanity { get; private set; }
        public int TilesanitySize { get; private set; }
#endif

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, ILogger logger, TesterFeatures testerFeatures)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _logger = logger;
            var slotDataReader = new SlotDataReader(_logger, _slotDataFields);

            Goal = slotDataReader.GetSlotSetting(SlotDataKeys.GOAL, Goal.CommunityCenter);
            var farmType = slotDataReader.GetSlotSetting(SlotDataKeys.FARM_TYPE, SupportedFarmType.Standard);
            FarmType = new FarmType(farmType);
            StartingMoney = slotDataReader.GetSlotSetting(SlotDataKeys.STARTING_MONEY, 500);
            ProfitMargin = slotDataReader.GetSlotSetting(SlotDataKeys.PROFIT_MARGIN, 100) / 100.0;
            BundlesData = slotDataReader.GetSlotSetting(SlotDataKeys.MODIFIED_BUNDLES, "");
            EntranceRandomization = slotDataReader.GetSlotSetting(SlotDataKeys.ENTRANCE_RANDOMIZATION, EntranceRandomization.Disabled);
            SeasonRandomization = slotDataReader.GetSlotSetting(SlotDataKeys.SEASON_RANDOMIZATION, SeasonRandomization.Disabled);
            Cropsanity = slotDataReader.GetSlotSetting(SlotDataKeys.CROPSANITY, Cropsanity.Disabled);
            BackpackProgression = slotDataReader.GetSlotSetting(SlotDataKeys.BACKPACK_PROGRESSION, BackpackProgression.Progressive);
            BackpackSize = slotDataReader.GetSlotSetting(SlotDataKeys.BACKPACK_SIZE, 12);
            ToolProgression = slotDataReader.GetSlotSetting(SlotDataKeys.TOOL_PROGRESSION, ToolProgression.Progressive);
            ElevatorProgression = slotDataReader.GetSlotSetting(SlotDataKeys.ELEVATOR_PROGRESSION, ElevatorProgression.ProgressiveFromPreviousFloor);
            SkillProgression = slotDataReader.GetSlotSetting(SlotDataKeys.SKILLS_PROGRESSION, SkillsProgression.Progressive);
            BuildingProgression = slotDataReader.GetSlotSetting(SlotDataKeys.BUILDING_PROGRESSION, BuildingProgression.Progressive);
            FestivalLocations = slotDataReader.GetSlotSetting(SlotDataKeys.FESTIVAL_OBJECTIVES, FestivalLocations.Easy);
            ArcadeMachineLocations = slotDataReader.GetSlotSetting(SlotDataKeys.ARCADE_MACHINES, ArcadeLocations.FullShuffling);
            SpecialOrderLocations = slotDataReader.GetSlotSetting(SlotDataKeys.SPECIAL_ORDERS, SpecialOrderLocations.Board);
            QuestLocations = new QuestLocations(slotDataReader.GetSlotSetting(SlotDataKeys.QUEST_LOCATIONS, 0));
            Fishsanity = slotDataReader.GetSlotSetting(SlotDataKeys.FISHSANITY, Fishsanity.None);
            Museumsanity = slotDataReader.GetSlotSetting(SlotDataKeys.MUSEUMSANITY, Museumsanity.None);
            Monstersanity = slotDataReader.GetSlotSetting(SlotDataKeys.MONSTERSANITY, Monstersanity.None);
            Shipsanity = slotDataReader.GetSlotSetting(SlotDataKeys.SHIPSANITY, Shipsanity.None);
            Cooksanity = slotDataReader.GetSlotSetting(SlotDataKeys.COOKSANITY, Cooksanity.None);
            Chefsanity = slotDataReader.GetSlotSetting(SlotDataKeys.CHEFSANITY, Chefsanity.Vanilla);
            Craftsanity = slotDataReader.GetSlotSetting(SlotDataKeys.CRAFTSANITY, Craftsanity.None);
            Friendsanity = slotDataReader.GetSlotSetting(SlotDataKeys.FRIENDSANITY, Friendsanity.None);
            FriendsanityHeartSize = slotDataReader.GetSlotSetting(SlotDataKeys.FRIENDSANITY_HEART_SIZE, 4);
            Eatsanity = slotDataReader.GetSlotEatsanitySetting();
            Booksanity = slotDataReader.GetSlotSetting(SlotDataKeys.BOOKSANITY, Booksanity.None);
            Walnutsanity = slotDataReader.GetSlotWalnutsanitySetting();
            Moviesanity = slotDataReader.GetSlotSetting(SlotDataKeys.MOVIESANITY, Moviesanity.None);
            Secretsanity = slotDataReader.GetSlotSecretsanitySetting();
            Hatsanity = slotDataReader.GetSlotSetting(SlotDataKeys.HATSANITY, Hatsanity.None);
            IncludeEndgameLocations = slotDataReader.GetSlotSetting(SlotDataKeys.INCLUDE_ENDGAME_LOCATIONS, false);
            ExcludeGingerIsland = slotDataReader.GetSlotSetting(SlotDataKeys.EXCLUDE_GINGER_ISLAND, true);
            TrapItemsDifficulty = slotDataReader.GetSlotSetting(SlotDataKeys.TRAP_DIFFICULTY, TrapItemsDifficulty.Medium, SlotDataKeys.TRAP_ITEMS);
            EnableMultiSleep = slotDataReader.GetSlotSetting(SlotDataKeys.MULTI_SLEEP_ENABLED, true);
            MultiSleepCostPerDay = slotDataReader.GetSlotSetting(SlotDataKeys.MULTI_SLEEP_COST, 0);
            ExperienceMultiplier = slotDataReader.GetSlotSetting(SlotDataKeys.EXPERIENCE_MULTIPLIER, 100) / 100.0;
            FriendshipMultiplier = slotDataReader.GetSlotSetting(SlotDataKeys.FRIENDSHIP_MULTIPLIER, 100) / 100.0;
            DebrisMultiplier = slotDataReader.GetSlotSetting(SlotDataKeys.DEBRIS_MULTIPLIER, DebrisMultiplier.HalfDebris);
            BundlePrice = slotDataReader.GetSlotSetting(SlotDataKeys.BUNDLE_PRICE, BundlePrice.Normal);
            QuickStart = slotDataReader.GetSlotSetting(SlotDataKeys.QUICK_START, false);
            Gifting = slotDataReader.GetSlotSetting(SlotDataKeys.GIFTING, true);
            Banking = true;
            DeathLink = slotDataReader.GetSlotSetting(SlotDataKeys.DEATH_LINK, false);
            Seed = slotDataReader.GetSlotSetting(SlotDataKeys.SEED, "");
            MultiworldVersion = slotDataReader.GetSlotSetting(SlotDataKeys.MULTIWORLD_VERSION, "");
            var newEntrancesStringData = slotDataReader.GetSlotSetting(SlotDataKeys.MODIFIED_ENTRANCES, "");
            ModifiedEntrances = JsonConvert.DeserializeObject<Dictionary<string, string>>(newEntrancesStringData);
            var modsString = slotDataReader.GetSlotSetting(SlotDataKeys.MOD_LIST, "");
            var mods = JsonConvert.DeserializeObject<List<string>>(modsString);
            Mods = new ModsManager(_logger, testerFeatures, mods);
            Jojapocalypse = new JojapocalypseSlotData(_logger, slotDataReader);

#if TILESANITY
            Tilesanity = slotDataReader.GetSlotSetting(SlotDataKeys.TILESANITY, Tilesanity.Nope);
            TilesanitySize = slotDataReader.GetSlotSetting(SlotDataKeys.TILESANITY_SIZE, 1);
#endif
        }

        public double ToolPriceMultiplier
        {
            get
            {
                if (ToolProgression.HasFlag(ToolProgression.VeryCheap))
                {
                    return 0.2;
                }

                if (ToolProgression.HasFlag(ToolProgression.Cheap))
                {
                    return 0.4;
                }

                return 1;
            }
        }

        public double BuildingPriceMultiplier
        {
            get
            {
                if (BuildingProgression.HasFlag(BuildingProgression.VeryCheap))
                {
                    return 0.2;
                }

                if (BuildingProgression.HasFlag(BuildingProgression.Cheap))
                {
                    return 0.5;
                }

                return 1;
            }
        }

        public double GetCurrentProfitMargin()
        {
            return ShippingConsequences.AdjustProfitMargin(ProfitMargin);
        }
    }
}
