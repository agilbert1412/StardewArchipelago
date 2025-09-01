namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    public enum Hatsanity
    {
        None =           0b0000000,
        Tailoring =      0b0000001,
        Easy =           0b0000010,
        Medium =         0b0000100,
        Difficult =      0b0001000,
        RNG =            0b0010000,
        NearPerfection = 0b0100000,
        PostPerfection = 0b1000000,
        SimplePreset = Tailoring | Easy | Medium,
        DifficultPreset = Tailoring | Easy | Medium | Difficult,
        All = Tailoring | Easy | Medium | Difficult | RNG | NearPerfection | PostPerfection,
    }
}