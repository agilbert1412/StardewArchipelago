using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.CodeInjections.Modded.SVE
{
    public class SVEShopInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static ShopReplacer _shopReplacer;
        private static ShopStockGenerator _shopStockGenerator;
        private const string ALESIA_DAGGER = "Tempered Galaxy Dagger";
        private const string ISAAC_SWORD = "Tempered Galaxy Sword";
        private const string ISAAC_HAMMER = "Tempered Galaxy Hammer";

        private static readonly string[] craftsanityRecipes = {
            "Haste Elixir",
            "Armor Elixir",
            "Hero Elixir",
        };

        private static readonly string[] chefsanityRecipes = {
            "Big Bark Burger",
            "Glazed Butterfish",
            "Mixed Berry Pie",
            "Baked Berry Oatmeal",
            "Flower Cookie",
            "Frog Legs",
            "Mushroom Berry Rice",
            "Seaweed Salad",
            "Void Delight",
            "Void Salmon Sushi",
        };

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, ShopReplacer shopReplacer, ShopStockGenerator shopStockGenerator)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _shopReplacer = shopReplacer;
            _shopStockGenerator = shopStockGenerator;
        }

        private static ShopMenu _lastShopMenuUpdated = null;

        // public override void update(GameTime time)
        public static void Update_ReplaceSVEShopChecks_Postfix(ShopMenu __instance, GameTime time)
        {
            try
            {
                // We only run this once for each menu
                if (_lastShopMenuUpdated == __instance)
                {
                    return;
                }

                _lastShopMenuUpdated = __instance;
                var myActiveHints = _archipelago.GetMyActiveHints();
                foreach (var salableItem in __instance.itemPriceAndStock.Keys.ToArray())
                {
                    ReplaceTemperedGalaxyWeapons(__instance, salableItem, myActiveHints);
                    ReplaceCraftsanityRecipes(__instance, salableItem, myActiveHints);
                    ReplaceChefsanityRecipes(__instance, salableItem, myActiveHints);
                }

                __instance.forSale = __instance.itemPriceAndStock.Keys.ToList();
                return; //  run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Update_ReplaceSVEShopChecks_Postfix)}:\n{ex}", LogLevel.Error);
                return; // run original logic
            }
        }

        private static void ReplaceTemperedGalaxyWeapons(ShopMenu shopMenu, ISalable salableItem, Hint[] myActiveHints)
        {
            _shopReplacer.ReplaceShopItem(shopMenu.itemPriceAndStock, salableItem, ALESIA_DAGGER, "Tempered Galaxy Dagger", myActiveHints);
            _shopReplacer.ReplaceShopItem(shopMenu.itemPriceAndStock, salableItem, ISAAC_SWORD, "Tempered Galaxy Sword", myActiveHints);
            _shopReplacer.ReplaceShopItem(shopMenu.itemPriceAndStock, salableItem, ISAAC_HAMMER, "Tempered Galaxy Hammer", myActiveHints);
        }

        private static void ReplaceCraftsanityRecipes(ShopMenu shopMenu, ISalable salableItem, Hint[] myActiveHints)
        {
            if (!_archipelago.SlotData.Craftsanity.HasFlag(Craftsanity.All))
            {
                return;
            }

            foreach (var recipeItem in craftsanityRecipes)
            {
                _shopReplacer.ReplaceShopRecipe(shopMenu.itemPriceAndStock, salableItem, $"{recipeItem} Recipe", recipeItem, myActiveHints);
            }
        }

        private static void ReplaceChefsanityRecipes(ShopMenu shopMenu, ISalable salableItem, Hint[] myActiveHints)
        {
            if (!_archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
            {
                return;
            }

            foreach (var recipeItem in chefsanityRecipes)
            {
                _shopReplacer.ReplaceShopRecipe(shopMenu.itemPriceAndStock, salableItem, $"{recipeItem} Recipe", recipeItem, myActiveHints);
            }
        }


        // Done as JojaMart was changed to be two different shop tenders (Claire and Martin); just force every shop in Joja to be the same.
        // public ShopMenu(Dictionary<ISalable, int[]> itemPriceAndStock, int currency = 0, string who = null, Func<ISalable, Farmer, int, bool> on_purchase = null, Func<ISalable, bool> on_sell = null, string context = null)
        public static bool Constructor_MakeBothJojaShopsTheSame_Prefix(ShopMenu __instance, ref Dictionary<ISalable, int[]> itemPriceAndStock, int currency, string who, Func<ISalable, Farmer, int, bool> on_purchase, Func<ISalable, bool> on_sell, string context)
        {
            try
            {
                if (Game1.currentLocation is not JojaMart)
                {
                    return true; // Run original logic
                }

                itemPriceAndStock = _shopStockGenerator.GetJojaStock();
                return true; // run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Constructor_MakeBothJojaShopsTheSame_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic   
            }

        }
    }
}