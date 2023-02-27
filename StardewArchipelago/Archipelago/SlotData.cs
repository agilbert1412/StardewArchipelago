using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class SlotData
    {
        private const string GOAL_KEY = "goal";
        private const string STARTING_MONEY_KEY = "starting_money";
        private const string ENTRANCE_RANDOMIZATION_KEY = "entrance_randomization";
        private const string BACKPACK_PROGRESSION_KEY = "backpack_progression";
        private const string TOOL_PROGRESSION_KEY = "tool_progression";
        private const string ELEVATOR_PROGRESSION_KEY = "elevator_progression";
        private const string SKILLS_PROGRESSION_KEY = "skill_progression";
        private const string BUILDING_PROGRESSION_KEY = "building_progression";
        private const string ARCADE_MACHINES_KEY = "arcade_machine_progression";
        private const string HELP_WANTED_LOCATIONS_KEY = "help_wanted_locations";
        private const string FISHSANITY_KEY = "fishsanity";
        private const string MULTI_SLEEP_ENABLED_KEY = "multiple_day_sleep_enabled";
        private const string MULTI_SLEEP_COST_KEY = "multiple_day_sleep_cost";
        private const string EXPERIENCE_MULTIPLIER_KEY = "experience_multiplier";
        private const string DEBRIS_MULTIPLIER_KEY = "debris_multiplier";
        private const string QUICK_START_KEY = "quick_start";
        private const string GIFTING_KEY = "gifting";
        private const string GIFT_TAX_KEY = "gift_tax";
        private const string DEATH_LINK_KEY = "death_link";
        private const string SEED_KEY = "seed";
        private const string MODIFIED_BUNDLES_KEY = "modified_bundles";
        private const string MODIFIED_ENTRANCES_KEY = "randomized_entrances";
        private const string MULTIWORLD_VERSION_KEY = "client_version";

        private Dictionary<string, object> _slotDataFields;
        private IMonitor _console;

        public string SlotName { get; private set; }
        public Goal Goal { get; private set; }
        public int StartingMoney { get; private set; }
        public EntranceRandomization EntranceRandomization { get; private set; }
        public BackpackProgression BackpackProgression { get; private set; }
        public ToolProgression ToolProgression { get; private set; }
        public ElevatorProgression ElevatorProgression { get; private set; }
        public SkillsProgression SkillProgression { get; private set; }
        public BuildingProgression BuildingProgression { get; private set; }
        public ArcadeProgression ArcadeMachineProgression { get; private set; }
        public int HelpWantedLocationNumber { get; private set; }
        public Fishsanity Fishsanity { get; private set; }
        public bool EnableMultiSleep { get; private set; }
        public int MultiSleepCostPerDay { get; private set; }
        public double ExperienceMultiplier { get; private set; }
        public DebrisMultiplier DebrisMultiplier { get; private set; }
        public bool QuickStart { get; private set; }
        public bool Gifting { get; private set; }
        public double GiftTax { get; private set; }
        public bool DeathLink { get; private set; }
        public string Seed { get; private set; }
        public string MultiworldVersion { get; private set; }
        private Dictionary<string, string> ModifiedBundles { get; set; }
        public Dictionary<string, string> ModifiedEntrances { get; set; }

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, IMonitor console)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _console = console;

            Goal = GetSlotSetting(GOAL_KEY, Goal.CommunityCenter);
            StartingMoney = GetSlotSetting(STARTING_MONEY_KEY, 500);
            EntranceRandomization = GetSlotSetting(ENTRANCE_RANDOMIZATION_KEY, EntranceRandomization.Disabled);
            BackpackProgression = GetSlotSetting(BACKPACK_PROGRESSION_KEY, BackpackProgression.Progressive);
            ToolProgression = GetSlotSetting(TOOL_PROGRESSION_KEY, ToolProgression.Progressive);
            ElevatorProgression = GetSlotSetting(ELEVATOR_PROGRESSION_KEY, ElevatorProgression.ProgressiveFromPreviousFloor);
            SkillProgression = GetSlotSetting(SKILLS_PROGRESSION_KEY, SkillsProgression.Progressive);
            BuildingProgression = GetSlotSetting(BUILDING_PROGRESSION_KEY, BuildingProgression.Shuffled);
            ArcadeMachineProgression = GetSlotSetting(ARCADE_MACHINES_KEY, ArcadeProgression.FullShuffling);
            HelpWantedLocationNumber = GetSlotSetting(HELP_WANTED_LOCATIONS_KEY, 0);
            Fishsanity = GetSlotSetting(FISHSANITY_KEY, Fishsanity.None);
            EnableMultiSleep = GetSlotSetting(MULTI_SLEEP_ENABLED_KEY, true);
            MultiSleepCostPerDay = GetSlotSetting(MULTI_SLEEP_COST_KEY, 0);
            ExperienceMultiplier = GetSlotSetting(EXPERIENCE_MULTIPLIER_KEY, 100) / 100.0;
            DebrisMultiplier = GetSlotSetting(DEBRIS_MULTIPLIER_KEY, DebrisMultiplier.HalfDebris);
            QuickStart = GetSlotSetting(QUICK_START_KEY, false);
            Gifting = GetSlotSetting(GIFTING_KEY, true);
            GiftTax = GetSlotSetting(GIFT_TAX_KEY, 30) / 100.0;
            DeathLink = GetSlotSetting(DEATH_LINK_KEY, false);
            Seed = GetSlotSetting(SEED_KEY, "");
            MultiworldVersion = GetSlotSetting(MULTIWORLD_VERSION_KEY, "");
            var newBundleStringData = GetSlotSetting(MODIFIED_BUNDLES_KEY, "");
            ModifiedBundles = JsonConvert.DeserializeObject<Dictionary<string, string>>(newBundleStringData);
            var newEntrancesStringData = GetSlotSetting(MODIFIED_ENTRANCES_KEY, "");
            ModifiedEntrances = JsonConvert.DeserializeObject<Dictionary<string, string>>(newEntrancesStringData);
        }

        private T GetSlotSetting<T>(string key, T defaultValue) where T : struct, Enum, IConvertible
        {
            return _slotDataFields.ContainsKey(key) ? Enum.Parse<T>(_slotDataFields[key].ToString(), true) : GetSlotDefaultValue(key, defaultValue);
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
            return _slotDataFields.ContainsKey(key) && _slotDataFields[key] != null ? (bool)_slotDataFields[key] : GetSlotDefaultValue(key, defaultValue);
        }

        private T GetSlotDefaultValue<T>(string key, T defaultValue)
        {
            _console.Log($"SlotData did not contain expected key: \"{key}\"", LogLevel.Warn);
            return defaultValue;
        }

        private static Dictionary<string, string> vanillaBundleData = null;

        public void ReplaceAllBundles()
        {
            if (vanillaBundleData == null)
            {
                vanillaBundleData = Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles");
            }
            Game1.netWorldState.Value.SetBundleData(vanillaBundleData);
            foreach (var key in ModifiedBundles.Keys)
            {
                var oldBundle = Game1.netWorldState.Value.BundleData[key];
                var newBundle = ModifiedBundles[key];
                var oldBundleName = oldBundle.Split("/")[0];
                var newBundleName = newBundle.Split("/")[0];
                CommunityCenterInjections._bundleNames.Add(newBundleName, oldBundleName);
                Game1.netWorldState.Value.BundleData[key] = newBundle;
            }
        }

        public void ReplaceEntrances()
        {
            if (EntranceRandomization == EntranceRandomization.Disabled)
            {
                return;
            }

            foreach (var (original, replacement) in ModifiedEntrances)
            {
                var originalExists = Entrances.TryGetEntrance(original, out var originalEntrance);
                var replacementExists = Entrances.TryGetEntrance(replacement, out var replacementEntrance);
                if (!originalExists || !replacementExists)
                {
                    if (!originalExists)
                    {
                        _console.Log($"Entrance \"{original}\" not found. Could not apply randomization provided by the AP server", LogLevel.Warn);
                    }
                    if (!replacementExists)
                    {
                        _console.Log($"Entrance \"{replacement}\" not found. Could not apply randomization provided by the AP server", LogLevel.Warn);
                    }
                    continue;
                }

                originalEntrance.ReplaceWith(replacementEntrance);
                DoReplacementOnEquivalentAreasAsWell(originalEntrance, original, replacementEntrance);
            }
        }

        private static void DoReplacementOnEquivalentAreasAsWell(OneWayEntrance originalEntrance, string original,
            OneWayEntrance replacementEntrance)
        {
            foreach (var equivalentGroup in EquivalentWarps.EquivalentAreas)
            {
                ReplaceEquivalentEntrances(originalEntrance.OriginName, original, replacementEntrance, equivalentGroup);
                ReplaceEquivalentEntrances(originalEntrance.DestinationName, original, replacementEntrance, equivalentGroup);
            }
        }

        private static void ReplaceEquivalentEntrances(string locationName, string originalLocationName, OneWayEntrance replacementEntrance,
            string[] equivalentAreasGroup)
        {
            if (!equivalentAreasGroup.Contains(locationName))
            {
                return;
            }

            foreach (var equivalentArea in equivalentAreasGroup)
            {
                if (locationName == equivalentArea)
                {
                    continue;
                }

                var newWarpName = originalLocationName.Replace(locationName, equivalentArea);
                var newEntranceExists = Entrances.TryGetEntrance(newWarpName, out var newEntrance);
                if (newEntranceExists)
                {
                    newEntrance.ReplaceWith(replacementEntrance);
                }
            }
        }
    }

    public enum EntranceRandomization
    {
        Disabled = 0,
        PelicanTown = 1,
        NonProgression = 2,
        Buildings = 3,
        Everything = 4,
        Chaos = 4,
    }

    public enum BackpackProgression
    {
        Vanilla = 0,
        Progressive = 1,
        ProgressiveEarlyBackpack = 2
    }

    public enum ToolProgression
    {
        Vanilla = 0,
        Progressive = 1,
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
    }

    public enum BuildingProgression
    {
        Vanilla = 0,
        Shuffled = 1,
        ShuffledEarlyShippingBin = 2
    }

    public enum ArcadeProgression
    {
        Disabled = 0,
        Victories = 1,
        VictoriesEasy = 2,
        FullShuffling = 3,
    }

    public enum Fishsanity
    {
        None = 0,
        Legendaries = 1,
        Special = 2,
        RandomSelection = 3,
        All = 4,
    }

    public enum Goal
    {
        CommunityCenter = 0,
        GrandpaEvaluation = 1,
        BottomOfMines = 2,
        CrypticNote = 3,
        MasterAngler = 4,
    }

    public enum DebrisMultiplier
    {
        Vanilla = 0,
        HalfDebris = 1,
        QuarterDebris = 2,
        NoDebris = 3,
        StartClear = 4,
    }
}
