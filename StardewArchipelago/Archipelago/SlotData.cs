using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Archipelago
{
    public class SlotData
    {
        private const string STARTING_MONEY_KEY = "starting_money";
        private const string BACKPACK_PROGRESSION_KEY = "backpack_progression";
        private const string TOOL_PROGRESSION_KEY = "tool_progression";
        private const string EARLY_MINE_KEY = "the_mines_open";
        private const string DEATH_LINK_KEY = "death_link";
        private const string GOAL_KEY = "goal";
        private const string SEED_KEY = "seed";

        // public int ProgressionBalancing { get; private set; }
        // public int Accessibility { get; private set; }
        // public int ResourcePackMultiplier { get; private set; }
        // public int ResourcePackUtility { get; private set; }
        public int StartingMoney { get; private set; }
        public BackpackProgression BackpackProgression { get; private set; }
        public ToolProgression ToolProgression { get; private set; }
        public bool EarlyMine { get; private set; }
        public bool DeathLink { get; private set; }
        public Goal Goal { get; private set; }
        public string Seed { get; private set; }

        public SlotData(Dictionary<string, object> slotDataFields)
        {
            StartingMoney = slotDataFields.ContainsKey(STARTING_MONEY_KEY) ? (int)(long)slotDataFields[STARTING_MONEY_KEY] : 500;
            BackpackProgression = slotDataFields.ContainsKey(BACKPACK_PROGRESSION_KEY) ? (BackpackProgression)(long)slotDataFields[BACKPACK_PROGRESSION_KEY] : BackpackProgression.Progressive;
            ToolProgression = slotDataFields.ContainsKey(TOOL_PROGRESSION_KEY) ? (ToolProgression)(long)slotDataFields[TOOL_PROGRESSION_KEY] : ToolProgression.Progressive;
            EarlyMine = slotDataFields.ContainsKey(EARLY_MINE_KEY) && (bool)slotDataFields[EARLY_MINE_KEY];
            DeathLink = slotDataFields.ContainsKey(DEATH_LINK_KEY) && (bool)slotDataFields[DEATH_LINK_KEY];
            Goal = slotDataFields.ContainsKey(GOAL_KEY) ? (Goal)(long)slotDataFields[GOAL_KEY] : Goal.CommunityCenter;
            Seed = slotDataFields.ContainsKey(SEED_KEY) ? slotDataFields[SEED_KEY].ToString() : "";
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

    public enum Goal
    {
        CommunityCenter = 0,
        GrandpaEvaluation = 1
    }
}
