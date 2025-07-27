using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;

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
