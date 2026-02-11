using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum EntranceRandomizationBehaviour
    {
        None =             0b0000,
        Decoupled =        0b0001,
        SameType =         0b0010,
        ShuffleFarmhouse = 0b0100,
        Chaos =            0b1000,
    }
}
