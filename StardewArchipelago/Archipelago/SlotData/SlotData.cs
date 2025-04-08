using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.GameModifications.Testing;
using StardewValley;
using StardewValley.GameData;

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
        public Booksanity Booksanity { get; private set; }
        public Walnutsanity Walnutsanity { get; private set; }
        public Secretsanity Secretsanity { get; private set; }
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

#if TILESANITY
        public Tilesanity Tilesanity { get; private set; }
        public int TilesanitySize { get; private set; }
#endif

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, ILogger logger, TesterFeatures testerFeatures)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _logger = logger;

            Goal = GetSlotSetting(SlotDataKeys.GOAL_KEY, Goal.CommunityCenter);
            var farmType = GetSlotSetting(SlotDataKeys.FARM_TYPE_KEY, SupportedFarmType.Standard);
            FarmType = new FarmType(farmType);
            StartingMoney = GetSlotSetting(SlotDataKeys.STARTING_MONEY_KEY, 500);
            ProfitMargin = GetSlotSetting(SlotDataKeys.PROFIT_MARGIN_KEY, 100) / 100.0;
            BundlesData = GetSlotSetting(SlotDataKeys.MODIFIED_BUNDLES_KEY, "");
            EntranceRandomization = GetSlotSetting(SlotDataKeys.ENTRANCE_RANDOMIZATION_KEY, EntranceRandomization.Disabled);
            SeasonRandomization = GetSlotSetting(SlotDataKeys.SEASON_RANDOMIZATION_KEY, SeasonRandomization.Disabled);
            Cropsanity = GetSlotSetting(SlotDataKeys.CROPSANITY_KEY, Cropsanity.Disabled);
            BackpackProgression = GetSlotSetting(SlotDataKeys.BACKPACK_PROGRESSION_KEY, BackpackProgression.Progressive);
            BackpackSize = GetSlotSetting(SlotDataKeys.BACKPACK_SIZE_KEY, 12);
            ToolProgression = GetSlotSetting(SlotDataKeys.TOOL_PROGRESSION_KEY, ToolProgression.Progressive);
            ElevatorProgression = GetSlotSetting(SlotDataKeys.ELEVATOR_PROGRESSION_KEY, ElevatorProgression.ProgressiveFromPreviousFloor);
            SkillProgression = GetSlotSetting(SlotDataKeys.SKILLS_PROGRESSION_KEY, SkillsProgression.Progressive);
            BuildingProgression = GetSlotSetting(SlotDataKeys.BUILDING_PROGRESSION_KEY, BuildingProgression.Progressive);
            FestivalLocations = GetSlotSetting(SlotDataKeys.FESTIVAL_OBJECTIVES_KEY, FestivalLocations.Easy);
            ArcadeMachineLocations = GetSlotSetting(SlotDataKeys.ARCADE_MACHINES_KEY, ArcadeLocations.FullShuffling);
            SpecialOrderLocations = GetSlotSetting(SlotDataKeys.SPECIAL_ORDERS_KEY, SpecialOrderLocations.Board);
            QuestLocations = new QuestLocations(GetSlotSetting(SlotDataKeys.QUEST_LOCATIONS_KEY, 0));
            Fishsanity = GetSlotSetting(SlotDataKeys.FISHSANITY_KEY, Fishsanity.None);
            Museumsanity = GetSlotSetting(SlotDataKeys.MUSEUMSANITY_KEY, Museumsanity.None);
            Monstersanity = GetSlotSetting(SlotDataKeys.MONSTERSANITY_KEY, Monstersanity.None);
            Shipsanity = GetSlotSetting(SlotDataKeys.SHIPSANITY_KEY, Shipsanity.None);
            Cooksanity = GetSlotSetting(SlotDataKeys.COOKSANITY_KEY, Cooksanity.None);
            Chefsanity = GetSlotSetting(SlotDataKeys.CHEFSANITY_KEY, Chefsanity.Vanilla);
            Craftsanity = GetSlotSetting(SlotDataKeys.CRAFTSANITY_KEY, Craftsanity.None);
            Friendsanity = GetSlotSetting(SlotDataKeys.FRIENDSANITY_KEY, Friendsanity.None);
            FriendsanityHeartSize = GetSlotSetting(SlotDataKeys.FRIENDSANITY_HEART_SIZE_KEY, 4);
            Booksanity = GetSlotSetting(SlotDataKeys.BOOKSANITY_KEY, Booksanity.None);
            Walnutsanity = GetSlotWalnutsanitySetting();
            Secretsanity = GetSlotSecretsanitySetting();
            ExcludeGingerIsland = GetSlotSetting(SlotDataKeys.EXCLUDE_GINGER_ISLAND_KEY, true);
            TrapItemsDifficulty = GetSlotSetting(SlotDataKeys.TRAP_DIFFICULTY_KEY, TrapItemsDifficulty.Medium, SlotDataKeys.TRAP_ITEMS_KEY);
            EnableMultiSleep = GetSlotSetting(SlotDataKeys.MULTI_SLEEP_ENABLED_KEY, true);
            MultiSleepCostPerDay = GetSlotSetting(SlotDataKeys.MULTI_SLEEP_COST_KEY, 0);
            ExperienceMultiplier = GetSlotSetting(SlotDataKeys.EXPERIENCE_MULTIPLIER_KEY, 100) / 100.0;
            FriendshipMultiplier = GetSlotSetting(SlotDataKeys.FRIENDSHIP_MULTIPLIER_KEY, 100) / 100.0;
            DebrisMultiplier = GetSlotSetting(SlotDataKeys.DEBRIS_MULTIPLIER_KEY, DebrisMultiplier.HalfDebris);
            BundlePrice = GetSlotSetting(SlotDataKeys.BUNDLE_PRICE_KEY, BundlePrice.Normal);
            QuickStart = GetSlotSetting(SlotDataKeys.QUICK_START_KEY, false);
            Gifting = GetSlotSetting(SlotDataKeys.GIFTING_KEY, true);
            Banking = true;
            DeathLink = GetSlotSetting(SlotDataKeys.DEATH_LINK_KEY, false);
            Seed = GetSlotSetting(SlotDataKeys.SEED_KEY, "");
            MultiworldVersion = GetSlotSetting(SlotDataKeys.MULTIWORLD_VERSION_KEY, "");
            var newEntrancesStringData = GetSlotSetting(SlotDataKeys.MODIFIED_ENTRANCES_KEY, "");
            ModifiedEntrances = JsonConvert.DeserializeObject<Dictionary<string, string>>(newEntrancesStringData);
            var modsString = GetSlotSetting(SlotDataKeys.MOD_LIST_KEY, "");
            var mods = JsonConvert.DeserializeObject<List<string>>(modsString);
            Mods = new ModsManager(_logger, testerFeatures, mods);

#if TILESANITY
            Tilesanity = GetSlotSetting(SlotDataKeys.TILESANITY_KEY, Tilesanity.Nope);
            TilesanitySize = GetSlotSetting(SlotDataKeys.TILESANITY_SIZE_KEY, 1);
#endif
        }

        private T GetSlotSetting<T>(string key, T defaultValue, params string[] alternateKeys) where T : struct, Enum, IConvertible
        {
            if (_slotDataFields.ContainsKey(key))
            {
                if (Enum.TryParse<T>(_slotDataFields[key].ToString(), true, out var parsedValue))
                {
                    return parsedValue;
                }
            }

            foreach (var alternateKey in alternateKeys)
            {
                if (_slotDataFields.ContainsKey(alternateKey))
                {
                    if (Enum.TryParse<T>(_slotDataFields[alternateKey].ToString(), true, out var parsedValue))
                    {
                        return parsedValue;
                    }
                }
            }

            return GetSlotDefaultValue(key, defaultValue);
        }

        private string GetSlotSetting(string key, string defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? _slotDataFields[key].ToString() : GetSlotDefaultValue(key, defaultValue);
        }

        private int GetSlotSetting(string key, int defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? (int)(long)_slotDataFields[key] : GetSlotDefaultValue(key, defaultValue);
        }

        private bool GetSlotSetting(string key, bool defaultValue)
        {
            if (_slotDataFields.ContainsKey(key) && _slotDataFields[key] != null && _slotDataFields[key] is bool boolValue)
            {
                return boolValue;
            }
            if (_slotDataFields[key] is string strValue && bool.TryParse(strValue, out var parsedValue))
            {
                return parsedValue;
            }
            if (_slotDataFields[key] is int intValue)
            {
                return intValue != 0;
            }
            if (_slotDataFields[key] is long longValue)
            {
                return longValue != 0;
            }
            if (_slotDataFields[key] is short shortValue)
            {
                return shortValue != 0;
            }

            return GetSlotDefaultValue(key, defaultValue);
        }

        private T GetSlotDefaultValue<T>(string key, T defaultValue)
        {
            _logger.LogWarning($"SlotData did not contain expected key: \"{key}\"");
            return defaultValue;
        }

        private Walnutsanity GetSlotWalnutsanitySetting()
        {
            return GetSlotOptionSetSetting<Walnutsanity>(SlotDataKeys.WALNUTSANITY_KEY);

            var walnutsanityValues = Walnutsanity.None;
            var walnutsanityJson = GetSlotSetting(SlotDataKeys.WALNUTSANITY_KEY, "");
            if (string.IsNullOrWhiteSpace(walnutsanityJson))
            {
                return walnutsanityValues;
            }
            var walnutsanityItems = JsonConvert.DeserializeObject<List<string>>(walnutsanityJson);
            if (walnutsanityItems == null)
            {
                return walnutsanityValues;
            }

            walnutsanityItems = walnutsanityItems.Select(x => x.Replace(" ", "")).ToList();
            foreach (var walnutsanityValue in Enum.GetValues<Walnutsanity>())
            {
                if (walnutsanityItems.Contains(walnutsanityValue.ToString()))
                {
                    walnutsanityValues |= walnutsanityValue;
                }
            }
            return walnutsanityValues;
        }

        private Secretsanity GetSlotSecretsanitySetting()
        {
            return GetSlotOptionSetSetting<Secretsanity>(SlotDataKeys.SECRETSANITY_KEY);
        }

        public TEnum GetSlotOptionSetSetting<TEnum>(string key) where TEnum : struct, Enum
        {
            var enabledValues = 0;
            var slotJson = GetSlotSetting(key, "");
            if (string.IsNullOrWhiteSpace(slotJson))
            {
                return (TEnum)(object)enabledValues;
            }
            var slotItems = JsonConvert.DeserializeObject<List<string>>(slotJson);
            if (slotItems == null)
            {
                return (TEnum)(object)enabledValues;
            }

            slotItems = slotItems.Select(x => x.Replace(" ", "")).ToList();
            foreach (var enumValue in Enum.GetValues<TEnum>())
            {
                if (slotItems.Contains(enumValue.ToString()))
                {
                    enabledValues |= (int)(object)enumValue;
                }
            }

            return (TEnum)(object)enabledValues;
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
    }

    public enum Goal
    {
        CommunityCenter = 0,
        GrandpaEvaluation = 1,
        BottomOfMines = 2,
        CrypticNote = 3,
        MasterAngler = 4,
        CompleteCollection = 5,
        FullHouse = 6,
        GreatestWalnutHunter = 7,
        ProtectorOfTheValley = 8,
        FullShipment = 9,
        GourmetChef = 10,
        CraftMaster = 11,
        Legend = 12,
        MysteryOfTheStardrops = 13,
        Allsanity = 24,
        Perfection = 25,
    }

    public enum SupportedFarmType
    {
        Standard = 0,
        Riverland = 1,
        Forest = 2,
        HillTop = 3,
        Wilderness = 4,
        FourCorners = 5,
        Beach = 6,
        Meadowlands = 7,
    }

    public class FarmType
    {
        private readonly SupportedFarmType _supportedFarmType;

        internal FarmType(SupportedFarmType supportedFarmType)
        {
            _supportedFarmType = supportedFarmType;
        }

        public SupportedFarmType GetFarmType()
        {
            return _supportedFarmType;
        }

        public int GetWhichFarm()
        {
            return (int)_supportedFarmType;
        }

        public ModFarmType GetWhichModFarm()
        {
            switch (_supportedFarmType)
            {
                case SupportedFarmType.Meadowlands:
                    var modFarmTypeList = DataLoader.AdditionalFarms(Game1.content);
                    return modFarmTypeList.First(x => x.Id.Contains(_supportedFarmType.ToString(), StringComparison.InvariantCultureIgnoreCase));
                case SupportedFarmType.Standard:
                case SupportedFarmType.Riverland:
                case SupportedFarmType.Forest:
                case SupportedFarmType.HillTop:
                case SupportedFarmType.Wilderness:
                case SupportedFarmType.FourCorners:
                case SupportedFarmType.Beach:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException($"Unrecognized SupportedFarmType Type: {_supportedFarmType}");
            }
        }

        public bool GetSpawnMonstersAtNight()
        {
            return _supportedFarmType == SupportedFarmType.Wilderness;
        }
    }

    public enum EntranceRandomization
    {
        Disabled = 0,
        PelicanTown = 1,
        NonProgression = 2,
        BuildingsWithoutHouse = 3,
        Buildings = 4,
        Everything = 10,
        Chaos = 12,
    }

    public enum SeasonRandomization
    {
        Disabled = 0,
        Randomized = 1,
        RandomizedNotWinter = 2,
        Progressive = 3,
    }

    public enum Cropsanity
    {
        Disabled = 0,
        Shuffled = 1,
    }

    public enum BackpackProgression
    {
        Vanilla = 0,
        Progressive = 1,
        ProgressiveEarlyBackpack = 2,
    }

    [Flags]
    public enum ToolProgression
    {
        // Vanilla = 0b0000,
        Progressive = 0b0001,
        Cheap = 0b0010,
        VeryCheap = 0b0100,
        NoStartingTools = 0b1000,
    }

    public enum ElevatorProgression
    {
        Vanilla = 0,
        Progressive = 1,
        ProgressiveFromPreviousFloor = 2,
    }

    public enum SkillsProgression
    {
        Vanilla = 0,
        Progressive = 1,
        ProgressiveWithMasteries = 2,
    }

    [Flags]
    public enum BuildingProgression
    {
        Progressive = 0b001,
        Cheap = 0b010,
        VeryCheap = 0b100,
    }

    public enum FestivalLocations
    {
        Vanilla = 0,
        Easy = 1,
        Hard = 2,
    }

    public enum ArcadeLocations
    {
        Disabled = 0,
        Victories = 1,
        VictoriesEasy = 2,
        FullShuffling = 3,
    }

    [Flags]
    public enum SpecialOrderLocations
    {
        Vanilla = 0b0000, // 0
        Board = 0b0001, // 1
        Qi = 0b0010, // 2
        Short = 0b0100, // 4
        VeryShort = 0b1000, // 8
    }

    public class QuestLocations
    {
        private readonly int _value;
        public bool StoryQuestsEnabled => _value >= 0;
        public int HelpWantedNumber => Math.Max(0, _value);

        internal QuestLocations(int value)
        {
            _value = value;
        }
    }

    public enum Fishsanity
    {
        None = 0,
        Legendaries = 1,
        Special = 2,
        RandomSelection = 3,
        All = 4,
        ExcludeLegendaries = 5,
        ExcludeHardFish = 6,
        OnlyEasyFish = 7,
    }

    public enum Museumsanity
    {
        None = 0,
        Milestones = 1,
        RandomSelection = 2,
        All = 3,
    }

    public enum Monstersanity
    {
        None = 0,
        OnePerCategory = 1,
        OnePerMonster = 2,
        Goals = 3,
        ShortGoals = 4,
        VeryShortGoals = 5,
        ProgressiveGoals = 6,
        SplitGoals = 7,
    }

    public enum Shipsanity
    {
        None = 0,
        Crops = 1,
        Fish = 3,
        FullShipment = 5,
        FullShipmentWithFish = 7,
        Everything = 9,
    }

    public enum Cooksanity
    {
        None = 0,
        QueenOfSauce = 1,
        All = 2,
    }

    [Flags]
    public enum Chefsanity
    {
        Vanilla = 0b0000,
        QueenOfSauce = 0b0001,
        Purchases = 0b0010,
        Skills = 0b0100,
        Friendship = 0b1000,
        All = QueenOfSauce | Purchases | Skills | Friendship,
    }

    public enum Craftsanity
    {
        None = 0,
        All = 1,
    }

    public enum Friendsanity
    {
        None = 0,
        // MarryOnePerson = 1,
        Bachelors = 2,
        StartingNpcs = 3,
        All = 4,
        AllWithMarriage = 5,
    }

    public enum Booksanity
    {
        None = 0,
        Power = 1,
        PowerSkill = 2,
        All = 3,
    }

    [Flags]
    public enum Walnutsanity
    {
        None = 0b0000,
        Puzzles = 0b0001,
        Bushes = 0b0010,
        DigSpots = 0b0100,
        Repeatables = 0b1000,
        All = Puzzles | Bushes | DigSpots | Repeatables,
    }

    [Flags]
    public enum Secretsanity
    {
        None = 0b0000,
        Easy = 0b0001,
        Difficult = 0b0010,
        Fishing = 0b0100,
        SecretNotes = 0b1000,
        All = Easy | Difficult | Fishing | SecretNotes,
    }

    public enum TrapItemsDifficulty
    {
        NoTraps = 0,
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Hell = 4,
        Nightmare = 5,
    }

    public enum DebrisMultiplier
    {
        Vanilla = 0,
        HalfDebris = 1,
        QuarterDebris = 2,
        NoDebris = 3,
        StartClear = 4,
    }

    public enum BundlePrice
    {
        Minimum = 0,
        VeryCheap = 1,
        Cheap = 2,
        Normal = 3,
        Expensive = 4,
        VeryExpensive = 5,
        Maximum = 6,
    }

#if TILESANITY
    public enum Tilesanity
    {
        Nope = 0,
        Locations = 1,
        Simplified = 2,
        Full = 3,
    }
#endif
}
