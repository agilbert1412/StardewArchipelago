using System;
using System.Collections.Generic;

namespace StardewArchipelago.Archipelago
{
    public class SlotData
    {
        private const string STARTING_MONEY_KEY = "starting_money";
        private const string BACKPACK_PROGRESSION_KEY = "backpack_progression";
        private const string TOOL_PROGRESSION_KEY = "tool_progression";
        private const string ELEVATOR_PROGRESSION_KEY = "elevator_progression";
        private const string SKILLS_PROGRESSION_KEY = "skill_progression";
        private const string EXPERIENCE_MULTIPLIER_KEY = "experience_multiplier";
        private const string EARLY_MINE_KEY = "the_mines_open";
        private const string DEATH_LINK_KEY = "death_link";
        private const string GOAL_KEY = "goal";
        private const string SEED_KEY = "seed";
        private const string MULTI_SLEEP_ENABLED_KEY = "multiple_day_sleep_enabled";
        private const string MULTI_SLEEP_COST_KEY = "multiple_day_sleep_cost";
        private const string QUICK_START_KEY = "quick_start";

        public string SlotName { get; private set; }
        private Dictionary<string, object> _slotDataFields;
        public int StartingMoney { get; private set; }
        public BackpackProgression BackpackProgression { get; private set; }
        public ToolProgression ToolProgression { get; private set; }
        public ElevatorProgression ElevatorProgression { get; private set; }
        public SkillsProgression SkillsProgression { get; private set; }
        public double ExperienceMultiplier { get; private set; }
        public bool EarlyMine { get; private set; }
        public bool DeathLink { get; private set; }
        public Goal Goal { get; private set; }
        public string Seed { get; private set; }
        public bool EnableMultiSleep { get; private set; }
        public int MultiSleepCostPerDay { get; private set; }
        public bool QuickStart { get; private set; }

        public SlotData(string slotName, Dictionary<string, object> slotDataFields)
        {
            SlotName = slotName;
            _slotDataFields = slotDataFields;
            StartingMoney = GetSlotSetting(STARTING_MONEY_KEY, 500);
            BackpackProgression = GetSlotSetting(BACKPACK_PROGRESSION_KEY, BackpackProgression.Progressive);
            ToolProgression = GetSlotSetting(TOOL_PROGRESSION_KEY, ToolProgression.Progressive);
            ElevatorProgression = GetSlotSetting(ELEVATOR_PROGRESSION_KEY, ElevatorProgression.ProgressiveFromPreviousFloor);
            SkillsProgression = GetSlotSetting(SKILLS_PROGRESSION_KEY, SkillsProgression.Progressive);
            ExperienceMultiplier = (GetSlotSetting(EXPERIENCE_MULTIPLIER_KEY, 100) / 100.0);
            EarlyMine = GetSlotSetting(EARLY_MINE_KEY, false);
            DeathLink = GetSlotSetting(DEATH_LINK_KEY, false);
            Goal = GetSlotSetting(GOAL_KEY, Goal.CommunityCenter);
            Seed = GetSlotSetting(SEED_KEY, "");
            EnableMultiSleep = GetSlotSetting(MULTI_SLEEP_ENABLED_KEY, true);
            MultiSleepCostPerDay = GetSlotSetting(MULTI_SLEEP_COST_KEY, 0);
            QuickStart = GetSlotSetting(QUICK_START_KEY, false); ;
        }

        private T GetSlotSetting<T>(string key, T defaultValue) where T : struct, Enum, IConvertible
        {
            return _slotDataFields.ContainsKey(key) ? Enum.Parse<T>(_slotDataFields[key].ToString(), true) : defaultValue;
        }

        private string GetSlotSetting(string key, string defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? _slotDataFields[key].ToString() : defaultValue;
        }

        private int GetSlotSetting(string key, int defaultValue)
        {
            return _slotDataFields.ContainsKey(key) ? (int)(long)_slotDataFields[key] : defaultValue;
        }

        private bool GetSlotSetting(string key, bool defaultValue)
        {
            return _slotDataFields.ContainsKey(EARLY_MINE_KEY) && _slotDataFields[EARLY_MINE_KEY] != null ? (bool)_slotDataFields[EARLY_MINE_KEY] : defaultValue;
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
        World = 2,
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

    public enum Goal
    {
        CommunityCenter = 0,
        GrandpaEvaluation = 1
    }
}
