using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum StartWithout
    {
        None = 0b0000,
        Tools = 0b0001,
        Backpack = 0b0010,
        Landslide = 0b0100,
        CommunityCenter = 0b1000,
        All = Tools | Backpack | Landslide | CommunityCenter,
    }
}