using System;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums
{
    [Flags]
    public enum Chefsanity
    {
        Vanilla = 0b0000,
        QueenOfSauce = 0b0001,
        Purchases = 0b0010,
        Skills = 0b0100,
        Friendship = 0b1000,
        All = QueenOfSauce | Purchases | Skills | Friendship,
    }
}