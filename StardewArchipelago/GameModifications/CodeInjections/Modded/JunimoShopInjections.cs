using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.CodeInjections.Modded
{
    public class JunimoShopInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static ShopStockGenerator _shopStockGenerator;
        private static StardewItemManager _stardewItemManager;
        private static JunimoShopGenerator _junimoShopGenerator;

        
        private static readonly List<string> _junimoFirstItems = new(){
            "Legend", "Prismatic Shard", "Ancient Seeds", "Dinosaur Egg", "Tiny Crop (stage 1)", "Super Starfruit", "Magic Bait"
        };

        private static readonly Dictionary<string, string> _junimoPhrase = new(){
            {"Orange", "Look! Me gib tasty \nfor orange thing!" },
            {"Red", "Give old things \nfor red gubbins!"},
            {"Grey", "I trade rocks for \n grey what's-its!"},
            {"Yellow", "I hab seeds, gib \nyellow gubbins!"},
            {"Blue", "I hab fish! You \ngive blue pretty?"}

        };
        private static readonly Dictionary<string, string> _firstItemToColor= new(){
            {"Legend", "Blue"}, {"Prismatic Shard", "Grey"}, {"Dinosaur Egg", "Red"}, {"Ancient Seeds", "Yellow"},
            {"Tiny Crop (stage 1)", "Orange"}, {"Super Starfruit", "Purple"}, {"Magic Bait", "Purple"}
        };

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, ShopStockGenerator shopStockGenerator, StardewItemManager stardewItemManager, JunimoShopGenerator junimoShopGenerator)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _shopStockGenerator = shopStockGenerator;
            _stardewItemManager = stardewItemManager;
            _junimoShopGenerator = junimoShopGenerator;
        }



        private static ShopMenu _lastShopMenuUpdated = null;
        
        // public override void update(GameTime time)
        public static bool Update_JunimoWoodsAPShop_Prefix(ShopMenu __instance, GameTime time)
        {
            try
            {
                // We only run this once for each menu
                if (__instance.storeContext != "Custom_JunimoWoods")
                {
                    return true;
                }
                var firstItemForSale = "";
                if (__instance.forSale.Count == 0) // in case the shop is emptied on a second pass
                {
                    firstItemForSale = "";
                }
                else
                {
                    firstItemForSale = __instance.forSale.First().Name; // Fighting CP moment
                }
                if (!_junimoFirstItems.Contains(firstItemForSale) && _lastShopMenuUpdated == __instance)
                {
                    return true;
                }
                _lastShopMenuUpdated = __instance;
                var color = _firstItemToColor[firstItemForSale];
                if (color == "Purple")
                {
                    // Will be worked on later
                    /*var purpleJunimo = __instance.portraitPerson;
                    __instance.exitThisMenuNoSound();
                    _junimoShopGenerator.PurpleJunimoSpecialShop(purpleJunimo);*/
                    return true;
                }
                __instance.itemPriceAndStock = _junimoShopGenerator.GetJunimoShopStock(color, __instance.itemPriceAndStock);
                __instance.forSale = __instance.itemPriceAndStock.Keys.ToList();
                __instance.potraitPersonDialogue = _junimoPhrase[color];
                return true; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Update_JunimoWoodsAPShop_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}