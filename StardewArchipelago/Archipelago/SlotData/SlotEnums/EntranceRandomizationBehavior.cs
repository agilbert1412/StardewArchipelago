using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum EntranceRandomizationBehavior
    {
        None =                     0b000000,
        Chaos =                    0b000001,
        Decoupled =                0b000010,
        SameDirection =            0b000100,
        SameType =                 0b001000,
        ShuffleFarmhouse =         0b010000,
        ShuffleFarmhouseAnywhere = 0b100000,
    }
}
