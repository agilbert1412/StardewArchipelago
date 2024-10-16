﻿namespace StardewArchipelago.Constants.Vanilla
{
    public static class Materials
    {
        public const string NONE = "";
        public const string COPPER = "Copper";
        public const string IRON = "Iron";
        public const string STEEL = "Steel";
        public const string GOLD = "Gold";
        public const string IRIDIUM = "Iridium";
        public static readonly string[] MaterialNames = { NONE, COPPER, IRON, GOLD, IRIDIUM };
        public static readonly string[] InternalMaterialNames = { NONE, COPPER, STEEL, GOLD, IRIDIUM };
        public static readonly int[] ToolUpgradeCosts = { 0, 2000, 5000, 10000, 25000 };
    }
}
