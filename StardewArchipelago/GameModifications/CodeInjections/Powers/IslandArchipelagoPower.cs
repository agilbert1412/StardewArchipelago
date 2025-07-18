using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;

namespace StardewArchipelago.GameModifications.CodeInjections.Powers
{
    public class IslandArchipelagoPower : ArchipelagoPower
    {
        public IslandArchipelagoPower(string name, string description = null, Point? textureOrigin = null) : base(name, description, textureOrigin, (x) => !x.ExcludeGingerIsland)
        {
        }
    }
}
