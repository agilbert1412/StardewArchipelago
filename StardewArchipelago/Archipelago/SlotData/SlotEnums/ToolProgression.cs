using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum ToolProgression
    {
        // Vanilla = 0b0000,
        Progressive = 0b0001,
        Cheap = 0b0010,
        VeryCheap = 0b0100,
        NoStartingTools = 0b1000,
    }
}