using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;

namespace StardewArchipelago.Items.Traps.Shuffle
{
    public class InventoryInfo
    {
        public string Map { get; set; }
        public Vector2 Tile { get; set; }

        public IList<Item> Content { get; set; }
        public int Capacity { get; set; }

        public InventoryInfo(string map, Vector2 tile, IList<Item> content, int capacity)
        {
            Map = map;
            Tile = tile;
            Content = content;
            Capacity = capacity;
        }
    }
}