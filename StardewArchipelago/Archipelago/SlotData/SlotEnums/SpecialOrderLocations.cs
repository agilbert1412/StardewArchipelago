using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum SpecialOrderLocations
    {
        Vanilla = 0b0000, // 0
        Board = 0b0001, // 1
        Qi = 0b0010, // 2
        Short = 0b0100, // 4
        VeryShort = 0b1000, // 8
    }
}