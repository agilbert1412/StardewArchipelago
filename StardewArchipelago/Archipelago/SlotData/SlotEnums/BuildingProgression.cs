using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum BuildingProgression
    {
        Progressive = 0b001,
        Cheap = 0b010,
        VeryCheap = 0b100,
    }
}