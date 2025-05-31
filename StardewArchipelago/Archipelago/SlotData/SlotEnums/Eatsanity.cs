using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum Eatsanity
    {
        None =           0b0000000,
        Crops =          0b0000001,
        Cooking =        0b0000010,
        Fish =           0b0000100,
        Artisan =        0b0001000,
        Shop =           0b0010000,
        Poisonous =      0b0100000,
        LockEffects =    0b1000000,
        All = Crops | Cooking | Fish | Artisan | Shop | Poisonous | LockEffects,
    }
}