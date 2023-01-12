using System.Collections.Generic;

namespace StardewArchipelago.Archipelago
{
    public class SlotData
    {
        private const string STARTING_MONEY_KEY = "starting_money";
        private const string BACKPACK_PROGRESSION_KEY = "backpack_progression";
        private const string TOOL_PROGRESSION_KEY = "tool_progression";
        private const string ELEVATOR_PROGRESSION_KEY = "elevator_progression";
        private const string EARLY_MINE_KEY = "the_mines_open";
        private const string DEATH_LINK_KEY = "death_link";
        private const string GOAL_KEY = "goal";
        private const string SEED_KEY = "seed";
        private const string MULTI_SLEEP_ENABLED_KEY = "multiple_day_sleep_enabled";
        private const string MULTI_SLEEP_COST_KEY = "multiple_day_sleep_cost";
        private const string QUICK_START_KEY = "quick_start";

        public string SlotName { get; private set; }
        public int StartingMoney { get; private set; }
        public BackpackProgression BackpackProgression { get; private set; }
        public ToolProgression ToolProgression { get; private set; }
        public ElevatorProgression ElevatorProgression { get; private set; }
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
            StartingMoney = slotDataFields.ContainsKey(STARTING_MONEY_KEY) ? (int)(long)slotDataFields[STARTING_MONEY_KEY] : 500;
            BackpackProgression = slotDataFields.ContainsKey(BACKPACK_PROGRESSION_KEY) ? (BackpackProgression)(long)slotDataFields[BACKPACK_PROGRESSION_KEY] : BackpackProgression.Progressive;
            ToolProgression = slotDataFields.ContainsKey(TOOL_PROGRESSION_KEY) ? (ToolProgression)(long)slotDataFields[TOOL_PROGRESSION_KEY] : ToolProgression.Progressive;
            ElevatorProgression = slotDataFields.ContainsKey(ELEVATOR_PROGRESSION_KEY) ? (ElevatorProgression)(long)slotDataFields[ELEVATOR_PROGRESSION_KEY] : ElevatorProgression.Progressive;
            EarlyMine = slotDataFields.ContainsKey(EARLY_MINE_KEY) && slotDataFields[EARLY_MINE_KEY] != null && (bool)slotDataFields[EARLY_MINE_KEY];
            DeathLink = slotDataFields.ContainsKey(DEATH_LINK_KEY) && slotDataFields[DEATH_LINK_KEY] != null && (bool)slotDataFields[DEATH_LINK_KEY];
            Goal = slotDataFields.ContainsKey(GOAL_KEY) ? (Goal)(long)slotDataFields[GOAL_KEY] : Goal.CommunityCenter;
            Seed = slotDataFields.ContainsKey(SEED_KEY) ? slotDataFields[SEED_KEY].ToString() : "";
            EnableMultiSleep = !slotDataFields.ContainsKey(MULTI_SLEEP_ENABLED_KEY) || slotDataFields[DEATH_LINK_KEY] == null || (bool)slotDataFields[MULTI_SLEEP_ENABLED_KEY];
            MultiSleepCostPerDay = slotDataFields.ContainsKey(MULTI_SLEEP_COST_KEY) ? (int)(long)slotDataFields[MULTI_SLEEP_COST_KEY] : 0;
            QuickStart = slotDataFields.ContainsKey(QUICK_START_KEY) && slotDataFields[QUICK_START_KEY] != null && (bool)slotDataFields[QUICK_START_KEY];
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
        ProgressiveFromPreviousFloor = 2
    }

    public enum Goal
    {
        CommunityCenter = 0,
        GrandpaEvaluation = 1
    }
}
