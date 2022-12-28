using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago
{
    public class SlotData
    {
        private const string STARTING_MONEY_KEY = "starting_money";
        private const string DEATH_LINK_KEY = "death_link";

        // public int ProgressionBalancing { get; private set; }
        // public int Accessibility { get; private set; }
        // public int ResourcePackMultiplier { get; private set; }
        // public int ResourcePackUtility { get; private set; }
        public int StartingMoney { get; private set; }
        public bool DeathLink { get; private set; }

        public SlotData(Dictionary<string, object> slotDataFields)
        {
            StartingMoney = slotDataFields.ContainsKey(STARTING_MONEY_KEY) ? (int)(long)slotDataFields[STARTING_MONEY_KEY] : 500;
            DeathLink = slotDataFields.ContainsKey(DEATH_LINK_KEY) ? (bool)slotDataFields[DEATH_LINK_KEY] : false;
        }
    }
}
