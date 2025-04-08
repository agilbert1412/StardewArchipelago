﻿using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.GameModifications.Testing;

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
}
