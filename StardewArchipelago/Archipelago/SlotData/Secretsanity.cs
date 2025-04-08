using System;

namespace StardewArchipelago.Archipelago.SlotData
{
    [Flags]
    public enum Secretsanity
    {
        None = 0b0000,
        Easy = 0b0001,
        Difficult = 0b0010,
        Fishing = 0b0100,
        SecretNotes = 0b1000,
        All = Easy | Difficult | Fishing | SecretNotes,
    }
}