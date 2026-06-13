using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum StartWithout
    {
        None = 0b000000,
        Tools = 0b000001,
        Backpack = 0b000010,
        Landslide = 0b000100,
        CommunityCenter = 0b001000,
        House = 0b010000,
        Villagers = 0b100000,
        All = Tools | Backpack | Landslide | CommunityCenter | House | Villagers,
    }
}