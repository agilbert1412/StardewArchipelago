using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Serialization
{
    public class TrapsStateDto
    {
        public int CurrentDebt { get; set; }
        public Dictionary<Buffs, int> CurrentBuffs { get; set; }
        public int DaysShunRemaining { get; set; }

        public TrapsStateDto()
        {
            CurrentDebt = 0;
            CurrentBuffs = new Dictionary<Buffs, int>();
            DaysShunRemaining = 0;
        }
    }
}
