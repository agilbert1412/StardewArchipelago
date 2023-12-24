using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.Items;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
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
            "Legend", "Prismatic Shard", "Ancient Seeds", "Dinosaur Egg"
        };
        private static readonly Dictionary<string, string> _firstItemToColor= new(){
            {"Legend", "Blue"}, {"Prismatic Shard", "Grey"}, {"Dinosaur Egg", "Red"}, {"Ancient Seeds", "Yellow"}
        };

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, ShopStockGenerator shopStockGenerator, StardewItemManager stardewItemManager)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _shopStockGenerator = shopStockGenerator;
            _stardewItemManager = stardewItemManager;
            _junimoShopGenerator = new JunimoShopGenerator(archipelago, shopStockGenerator, stardewItemManager);
        }



        private static ShopMenu _lastShopMenuUpdated = null;
        
        public static void Update_JunimoWoodsAPShop_Postfix(ShopMenu __instance, GameTime time)
        {
            try
            {
                // We only run this once for each menu
                if (__instance.storeContext != "Custom_JunimoWoods")
                {
                    return;
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
                    return;
                }
                _lastShopMenuUpdated = __instance;
                __instance.itemPriceAndStock = _junimoShopGenerator.GetJunimoShopStock(_firstItemToColor[firstItemForSale]);
                __instance.forSale = __instance.itemPriceAndStock.Keys.ToList();
                return; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Update_JunimoWoodsAPShop_Postfix)}:\n{ex}", LogLevel.Error);
                return; // run original logic
            }
        }
    }
}