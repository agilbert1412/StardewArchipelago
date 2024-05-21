using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.GameModifications.Modded
{
    public class JunimoShopStockModifier
    {
        private ArchipelagoClient _archipelago;
        private SeedShopStockModifier _seedShopStockModifier;
        private StardewItemManager _stardewItemManager;
        private Dictionary<int, int> BlueItems { get; set; }
        private static readonly List<string> BlueColors = new()
        {
            "color_blue", "color_aquamarine", "color_dark_blue", "color_cyan", "color_light_cyan", "color_dark_cyan",
        };
        private Dictionary<int, int> GreyItems { get; set; }
        private static readonly List<string> GreyColors = new()
        {
            "color_gray", "color_black", "color_poppyseed", "color_dark_gray",
        };
        private Dictionary<int, int> RedItems { get; set; }
        private static readonly List<string> RedColors = new()
        {
            "color_red", "color_pink", "color_dark_pink", "color_salmon",
        };
        private Dictionary<int, int> YellowItems { get; set; }
        private static readonly List<string> YellowColors = new()
        {
            "color_yellow", "color_gold", "color_sand", "color_dark_yellow",
        };
        private Dictionary<int, int> OrangeItems { get; set; }

        private static readonly List<string> OrangeColors = new()
        {
            "color_orange", "color_dark_orange", "color_dark_brown", "color_brown", "color_copper",
        };
        public Dictionary<int, int> PurpleItems { get; set; }
        private static readonly List<string> PurpleColors = new()
        {
            "color_purple", "color_dark_purple", "color_dark_pink", "color_pale_violet_red", "color_iridium",
        };
        public Dictionary<StardewItem, int> BerryItems { get; set; }
        private static readonly string[] spring = new string[] { "spring" };
        private static readonly string[] summer = new string[] { "summer" };
        private static readonly string[] fall = new string[] { "fall" };
    }
}