using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace StardewArchipelago.Archipelago
{
    public class SlotData
    {
        private const string GOAL_KEY = "goal";
        private const string STARTING_MONEY_KEY = "starting_money";
        private const string BACKPACK_PROGRESSION_KEY = "backpack_progression";
        private const string TOOL_PROGRESSION_KEY = "tool_progression";
        private const string ELEVATOR_PROGRESSION_KEY = "elevator_progression";
        private const string SKILLS_PROGRESSION_KEY = "skill_progression";
        private const string BUILDING_PROGRESSION_KEY = "building_progression";
        private const string ARCADE_MACHINES_KEY = "arcade_machine_progression";
        private const string MULTI_SLEEP_ENABLED_KEY = "multiple_day_sleep_enabled";
        private const string MULTI_SLEEP_COST_KEY = "multiple_day_sleep_cost";
        private const string EXPERIENCE_MULTIPLIER_KEY = "experience_multiplier";
        private const string DEBRIS_MULTIPLIER_KEY = "debris_multiplier";
        private const string QUICK_START_KEY = "quick_start";
        private const string DEATH_LINK_KEY = "death_link";
        private const string SEED_KEY = "seed";

        private Dictionary<string, object> _slotDataFields;
        private IMonitor _console;

        public string SlotName { get; private set; }
        public Goal Goal { get; private set; }
        public int StartingMoney { get; private set; }
        public BackpackProgression BackpackProgression { get; private set; }
        public ToolProgression ToolProgression { get; private set; }
        public ElevatorProgression ElevatorProgression { get; private set; }
        public SkillsProgression SkillProgression { get; private set; }
        public BuildingProgression BuildingProgression { get; private set; }
        public ArcadeProgression ArcadeMachineProgression { get; private set; }
        public bool EnableMultiSleep { get; private set; }
        public int MultiSleepCostPerDay { get; private set; }
        public double ExperienceMultiplier { get; private set; }
        public DebrisMultiplier DebrisMultiplier { get; private set; }
        public bool QuickStart { get; private set; }
        public bool DeathLink { get; private set; }
        public string Seed { get; private set; }

        public SlotData(string slotName, Dictionary<string, object> slotDataFields, IMonitor console)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            _console = console;

            Goal = GetSlotSetting(GOAL_KEY, Goal.CommunityCenter);
            StartingMoney = GetSlotSetting(STARTING_MONEY_KEY, 500);
            BackpackProgression = GetSlotSetting(BACKPACK_PROGRESSION_KEY, BackpackProgression.Progressive);
            ToolProgression = GetSlotSetting(TOOL_PROGRESSION_KEY, ToolProgression.Progressive);
            ElevatorProgression = GetSlotSetting(ELEVATOR_PROGRESSION_KEY, ElevatorProgression.ProgressiveFromPreviousFloor);
            SkillProgression = GetSlotSetting(SKILLS_PROGRESSION_KEY, SkillsProgression.Progressive);
            BuildingProgression = GetSlotSetting(BUILDING_PROGRESSION_KEY, BuildingProgression.Shuffled);
            ArcadeMachineProgression = GetSlotSetting(ARCADE_MACHINES_KEY, ArcadeProgression.FullShuffling);
            EnableMultiSleep = GetSlotSetting(MULTI_SLEEP_ENABLED_KEY, true);
            MultiSleepCostPerDay = GetSlotSetting(MULTI_SLEEP_COST_KEY, 0);
            ExperienceMultiplier = (GetSlotSetting(EXPERIENCE_MULTIPLIER_KEY, 100) / 100.0);
            DebrisMultiplier = GetSlotSetting(DEBRIS_MULTIPLIER_KEY, DebrisMultiplier.HalfDebris);
            QuickStart = GetSlotSetting(QUICK_START_KEY, false);
            DeathLink = GetSlotSetting(DEATH_LINK_KEY, false);
            Seed = GetSlotSetting(SEED_KEY, "");
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
    }

    public enum BackpackProgression
    {
        Vanilla = 0,
        Progressive = 1
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
        Shuffled = 1
    }

    public enum ArcadeProgression
    {
        Disabled = 0,
        Victories = 1,
        VictoriesEasy = 2,
        FullShuffling = 3,
    }

    public enum Goal
    {
        CommunityCenter = 0,
        GrandpaEvaluation = 1,
        BottomOfMines = 2
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
