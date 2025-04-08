using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum Walnutsanity
    {
        None = 0b0000,
        Puzzles = 0b0001,
        Bushes = 0b0010,
        DigSpots = 0b0100,
        Repeatables = 0b1000,
        All = Puzzles | Bushes | DigSpots | Repeatables,
    }
}