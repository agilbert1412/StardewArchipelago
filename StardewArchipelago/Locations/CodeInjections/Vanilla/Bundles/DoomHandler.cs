using System;
using System.Collections.Generic;
using System.Linq;
using ManagedDoom;
using ManagedDoom.Silk;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class DoomHandler
    {
        public const int IMAGE_SIZE = 256;
        private const int OFFSET_X = 936 - (IMAGE_SIZE / 2);
        private const int OFFSET_Y = 88;

        public DoomHandler()
        {
            var doom = new SilkDoom(new CommandLineArgs())
        }
    }
}
