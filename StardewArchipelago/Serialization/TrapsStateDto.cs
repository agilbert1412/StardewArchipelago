using System.Collections.Generic;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Serialization
{
    public class TrapsStateDto
    {
        public Dictionary<Buffs, int> CurrentBuffs { get; set; }
        public Dictionary<string, int> PariahShunning { get; set; }

        public TrapsStateDto()
        {
            CurrentBuffs = new Dictionary<Buffs, int>();
            PariahShunning = new Dictionary<string, int>();
        }
    }
}
