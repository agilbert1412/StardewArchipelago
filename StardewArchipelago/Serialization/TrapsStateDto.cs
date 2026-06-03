using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Serialization
{
    public class TrapsStateDto
    {
        public Dictionary<Buffs, int> CurrentBuffs { get; set; }
        public int MillisecondsShunRemaining { get; set; }

        public TrapsStateDto()
        {
            CurrentBuffs = new Dictionary<Buffs, int>();
            MillisecondsShunRemaining = 0;
        }
    }
}
