using Microsoft.Xna.Framework;

namespace StardewArchipelago.Items.Traps.Shuffle
{
    public class InventoryInfo
    {
        public string Map { get; set; }
        public Vector2 Tile { get; set; }

        public StardewValley.Inventories.Inventory Content { get; set; }
        public int Capacity { get; set; }

        public InventoryInfo(string map, Vector2 tile, StardewValley.Inventories.Inventory content, int capacity)
        {
            Map = map;
            Tile = tile;
            Content = content;
            Capacity = capacity;
        }
    }
}