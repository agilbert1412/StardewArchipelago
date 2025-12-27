using System;
using StardewArchipelago.Archipelago.SlotData;

namespace StardewArchipelago.GameModifications.CodeInjections.Powers
{
    public class VanillaArchipelagoPower : ArchipelagoPower
    {
        public string ApItemName { get; set; }
        public override string DisplayName => ApItemName;

        public VanillaArchipelagoPower(string name, string apItemName, Func<SlotData, bool> isIncluded = null) : base(name, null, null, isIncluded)
        {
            ApItemName = apItemName;
        }

        public override bool IsVanillaPower()
        {
            return true;
        }
    }
}
