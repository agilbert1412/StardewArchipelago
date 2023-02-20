using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Netcode;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class SeedShopsInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
            _archipelago = archipelago;
        }
        
        public static bool ShopStock_PierreSeasonal_Prefix(SeedShop __instance, ref Dictionary<ISalable, int[]> result)
        {
            try
            {
                var stock = new Dictionary<ISalable, int[]>();
                AddSeedsToPierreStock(stock);
                AddGrassStarterToPierreStock(stock);
                AddCookingIngredientsToPierreStock(stock);
                AddFertilizersToShop(stock);
                AddFurnitureToShop(stock);
                AddSaplingsToShop(stock);
                AddBuyBackToShop(__instance, stock);
                AddBouquetToShop(stock);

                result = stock;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ShopStock_PierreSeasonal_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // public static Dictionary<ISalable, int[]> getJojaStock()

        public static bool GetJojaStock_FullCostco_Prefix(ref Dictionary<ISalable, int[]> result)
        {
            try
            {

                var jojaStock = new Dictionary<ISalable, int[]>();
                // AutoPetter has been removed
                AddToJojaStock(jojaStock, JOJA_COLA, 75, 6);
                AddJojaFurnitureToShop(jojaStock);
                AddSeedsToJojaStock(jojaStock);
                AddGrassStarterToJojaStock(jojaStock);
                AddCookingIngredientsToJojaStock(jojaStock);

                result = jojaStock;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetJojaStock_FullCostco_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void AddSeedsToPierreStock(Dictionary<ISalable, int[]> stock)
        {
            AddSpringSeedsToPierreStock(stock);
            AddSummerSeedsToPierreStock(stock);
            AddFallSeedsToPierreStock(stock);
        }

        private static void AddSpringSeedsToPierreStock(Dictionary<ISalable, int[]> stock)
        {
            AddToPierreStock(stock, PARSNIP_SEEDS, itemSeason: "spring");
            AddToPierreStock(stock, BEAN_STARTER, itemSeason: "spring");
            AddToPierreStock(stock, CAULIFLOWER_SEEDS, itemSeason: "spring");
            AddToPierreStock(stock, POTATO_SEEDS, itemSeason: "spring");
            AddToPierreStock(stock, TULIP_BULB, itemSeason: "spring");
            AddToPierreStock(stock, KALE_SEEDS, itemSeason: "spring");
            AddToPierreStock(stock, JAZZ_SEEDS, itemSeason: "spring");
            AddToPierreStock(stock, GARLIC_SEEDS, itemSeason: "spring");
            AddToPierreStock(stock, RICE_SHOOT, itemSeason: "spring");
        }

        private static void AddSummerSeedsToPierreStock(Dictionary<ISalable, int[]> stock)
        {
            AddToPierreStock(stock, MELON_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, TOMATO_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, BLUEBERRY_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, PEPPER_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, WHEAT_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, RADISH_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, POPPY_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, SPANGLE_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, HOPS_STARTER, itemSeason: "summer");
            AddToPierreStock(stock, CORN_SEEDS, itemSeason: "summer");
            AddToPierreStock(stock, SUNFLOWER_SEEDS, 100, "summer");
            AddToPierreStock(stock, RED_CABBAGE_SEEDS, itemSeason: "summer");
        }

        private static void AddFallSeedsToPierreStock(Dictionary<ISalable, int[]> stock)
        {
            AddToPierreStock(stock, PUMPKIN_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, CORN_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, EGGPLANT_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, BOK_CHOY_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, YAM_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, CRANBERRY_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, WHEAT_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, SUNFLOWER_SEEDS, 100, "fall");
            AddToPierreStock(stock, FAIRY_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, AMARANTH_SEEDS, itemSeason: "fall");
            AddToPierreStock(stock, GRAPE_STARTER, itemSeason: "fall");
            AddToPierreStock(stock, ARTICHOKE_SEEDS, itemSeason: "fall");
        }

        private static void AddGrassStarterToPierreStock(Dictionary<ISalable, int[]> stock)
        {
            AddToPierreStock(stock, GRASS_STARTER);
            if (!Game1.player.craftingRecipes.ContainsKey("Grass Starter"))
            {
                stock.Add(new StardewValley.Object(GRASS_STARTER, 1, true), new int[2]
                {
                    1000,
                    1
                });
            }
        }

        private static void AddCookingIngredientsToPierreStock(Dictionary<ISalable, int[]> stock)
        {
            AddToPierreStock(stock, SUGAR);
            AddToPierreStock(stock, WHEAT_FLOUR);
            AddToPierreStock(stock, RICE);
            AddToPierreStock(stock, OIL);
            AddToPierreStock(stock, VINEGAR);
        }

        private static void AddFertilizersToShop(Dictionary<ISalable, int[]> stock)
        {
            if ((int)Game1.stats.DaysPlayed >= 15)
            {
                AddToPierreStock(stock, BASIC_FERTILIZER, 50);
                AddToPierreStock(stock, BASIC_RETAINING_SOIL, 50);
                AddToPierreStock(stock, SPEED_GRO, 50);
            }

            if (Game1.year > 1)
            {
                AddToPierreStock(stock, QUALITY_FERTILIZER, 75);
                AddToPierreStock(stock, QUALITY_RETAINING_SOIL, 75);
                AddToPierreStock(stock, DELUXE_SPEED_GRO, 75);
            }
        }

        private static void AddFurnitureToShop(Dictionary<ISalable, int[]> stock)
        {
            var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
            var which = random.Next(112);
            if (which == 21)
            {
                which = 36;
            }

            var key1 = new Wallpaper(which);
            stock.Add(key1, new int[2]
            {
                key1.salePrice(),
                int.MaxValue
            });
            var key2 = new Wallpaper(random.Next(56), true);
            stock.Add(key2, new int[2]
            {
                key2.salePrice(),
                int.MaxValue
            });
            var key3 = new Furniture(1308, Vector2.Zero);
            stock.Add(key3, new int[2]
            {
                key3.salePrice(),
                int.MaxValue
            });
        }

        private static void AddSaplingsToShop(Dictionary<ISalable, int[]> stock)
        {
            AddToPierreStock(stock, CHERRY_SAPLING, 1700);
            AddToPierreStock(stock, APRICOT_SAPLING, 1000);
            AddToPierreStock(stock, ORANGE_SAPLING, 2000);
            AddToPierreStock(stock, PEACH_SAPLING, 3000);
            AddToPierreStock(stock, POMEGRANATE_SAPLING, 3000);
            AddToPierreStock(stock, APPLE_SAPLING, 2000);
        }

        private static void AddBuyBackToShop(SeedShop __instance, Dictionary<ISalable, int[]> stock)
        {
            foreach (var itemToBuyBack in __instance.itemsFromPlayerToSell)
            {
                if (itemToBuyBack.Stack <= 0)
                {
                    continue;
                }
                var buyBackPrice = itemToBuyBack.salePrice();
                if (itemToBuyBack is StardewValley.Object objectToBuyBack)
                {
                    buyBackPrice = objectToBuyBack.sellToStorePrice();
                }

                stock.Add(itemToBuyBack, new int[2]
                {
                    buyBackPrice,
                    itemToBuyBack.Stack
                });
            }
        }

        private static void AddBouquetToShop(Dictionary<ISalable, int[]> stock)
        {
            if (Game1.player.hasAFriendWithHeartLevel(8, true))
            {
                AddToPierreStock(stock, BOUQUET);
            }
        }

        private static void AddToPierreStock(
            Dictionary<ISalable, int[]> stock,
            int itemId,
            int basePrice = -1,
            string itemSeason = null,
            int howManyInStock = -1)
        {
            var priceMultiplier = 2.0;
            var item = new StardewValley.Object(Vector2.Zero, itemId, 1);
            if (basePrice == -1)
            {
                basePrice = item.salePrice();
                priceMultiplier = 1f;
            }
            else if (item.isSapling())
            {
                priceMultiplier *= Game1.MasterPlayer.difficultyModifier;
            }

            if (itemSeason != null && itemSeason != Game1.currentSeason)
            {
                if (!Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist"))
                {
                    return;
                }

                priceMultiplier *= 1.5f;
            }
            var price = (int)(basePrice * priceMultiplier);
            if (itemSeason != null)
            {
                foreach (var (itemAlreadyInStock, saleDetails) in stock)
                {
                    if (itemAlreadyInStock is not StardewValley.Object objectInStock) continue;
                    if (!Utility.IsNormalObjectAtParentSheetIndex(objectInStock, itemId)) continue;
                    if (saleDetails.Length == 0 || price >= saleDetails[0])
                    {
                        return;
                    }

                    saleDetails[0] = price;
                    stock[objectInStock] = saleDetails;
                    return;
                }
            }

            if (howManyInStock == -1)
            {
                var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + itemId);
                howManyInStock = random.Next(20);
                if (howManyInStock < 5)
                {
                    return;
                }
            }
            stock.Add(item, new int[2]
            {
                price,
                howManyInStock
            });
        }

        private static void AddSeedsToJojaStock(Dictionary<ISalable, int[]> stock)
        {
            AddSpringSeedsToJojaStock(stock);
            AddSummerSeedsToJojaStock(stock);
            AddFallSeedsToJojaStock(stock);
        }

        private static void AddSpringSeedsToJojaStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJojaStock(stock, PARSNIP_SEEDS);
            AddToJojaStock(stock, BEAN_STARTER);
            AddToJojaStock(stock, CAULIFLOWER_SEEDS);
            AddToJojaStock(stock, POTATO_SEEDS);
            AddToJojaStock(stock, TULIP_BULB);
            AddToJojaStock(stock, KALE_SEEDS);
            AddToJojaStock(stock, JAZZ_SEEDS);
            AddToJojaStock(stock, GARLIC_SEEDS);
            AddToJojaStock(stock, RICE_SHOOT);
        }

        private static void AddSummerSeedsToJojaStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJojaStock(stock, MELON_SEEDS);
            AddToJojaStock(stock, TOMATO_SEEDS);
            AddToJojaStock(stock, BLUEBERRY_SEEDS);
            AddToJojaStock(stock, PEPPER_SEEDS);
            AddToJojaStock(stock, WHEAT_SEEDS);
            AddToJojaStock(stock, RADISH_SEEDS);
            AddToJojaStock(stock, POPPY_SEEDS);
            AddToJojaStock(stock, SPANGLE_SEEDS);
            AddToJojaStock(stock, HOPS_STARTER);
            AddToJojaStock(stock, CORN_SEEDS);
            AddToJojaStock(stock, SUNFLOWER_SEEDS);
            AddToJojaStock(stock, RED_CABBAGE_SEEDS);
        }

        private static void AddFallSeedsToJojaStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJojaStock(stock, PUMPKIN_SEEDS);
            AddToJojaStock(stock, EGGPLANT_SEEDS);
            AddToJojaStock(stock, BOK_CHOY_SEEDS);
            AddToJojaStock(stock, YAM_SEEDS);
            AddToJojaStock(stock, CRANBERRY_SEEDS);
            AddToJojaStock(stock, FAIRY_SEEDS);
            AddToJojaStock(stock, AMARANTH_SEEDS);
            AddToJojaStock(stock, GRAPE_STARTER);
            AddToJojaStock(stock, ARTICHOKE_SEEDS);
        }

        private static void AddGrassStarterToJojaStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJojaStock(stock, GRASS_STARTER);
        }

        private static void AddCookingIngredientsToJojaStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJojaStock(stock, SUGAR, packSize: 20);
            AddToJojaStock(stock, WHEAT_FLOUR, packSize: 20);
            AddToJojaStock(stock, RICE, packSize: 20);
            AddToJojaStock(stock, OIL, packSize: 20);
            AddToJojaStock(stock, VINEGAR, packSize: 20);
        }

        private static void AddToJojaStock(
            Dictionary<ISalable, int[]> stock,
            int itemId,
            int basePrice = -1,
            int packSize = 50)
        {
            var priceMultiplier = 1.0;
            var item = new StardewValley.Object(Vector2.Zero, itemId, packSize);
            if (basePrice == -1)
            {
                basePrice = item.salePrice();
                priceMultiplier = 0.80;
            }
            else if (item.isSapling())
            {
                priceMultiplier *= Game1.MasterPlayer.difficultyModifier;
            }

            var price = (int)(basePrice * priceMultiplier * packSize);
            stock.Add(item, new int[2]
            {
                price,
                int.MaxValue
            });
        }

        private static void AddJojaFurnitureToShop(Dictionary<ISalable, int[]> stock)
        {
            var key1 = new Wallpaper(21);
            key1.Stack = int.MaxValue;
            var numArray1 = new int[] { 20, int.MaxValue };
            stock.Add(key1, numArray1);
            var key2 = new Furniture(1609, Vector2.Zero);
            key2.Stack = int.MaxValue;
            var numArray2 = new int[] { 500, int.MaxValue };
            stock.Add(key2, numArray2);
        }

        private const int JOJA_COLA = 167;

        private const int SUGAR = 245;
        private const int WHEAT_FLOUR = 246;
        private const int OIL = 247;
        private const int RICE_SHOOT = 273;
        private const int GRASS_STARTER = 297;
        private const int AMARANTH_SEEDS = 299;
        private const int GRAPE_STARTER = 301;
        private const int HOPS_STARTER = 302;
        private const int VINEGAR = 419;
        private const int RICE = 423;
        private const int FAIRY_SEEDS = 425;
        private const int TULIP_BULB = 427;
        private const int JAZZ_SEEDS = 429;
        private const int SUNFLOWER_SEEDS = 431;
        private const int POPPY_SEEDS = 453;
        private const int SPANGLE_SEEDS = 455;
        private const int PARSNIP_SEEDS = 472;
        private const int BEAN_STARTER = 473;
        private const int CAULIFLOWER_SEEDS = 474;
        private const int POTATO_SEEDS = 475;
        private const int GARLIC_SEEDS = 476;
        private const int KALE_SEEDS = 477;
        private const int RHUBARB_SEEDS = 478;
        private const int MELON_SEEDS = 479;
        private const int TOMATO_SEEDS = 480;
        private const int BLUEBERRY_SEEDS = 481;
        private const int PEPPER_SEEDS = 482;
        private const int WHEAT_SEEDS = 483;
        private const int RADISH_SEEDS = 484;
        private const int RED_CABBAGE_SEEDS = 485;
        private const int STARFRUIT_SEEDS = 486;
        private const int CORN_SEEDS = 487;
        private const int EGGPLANT_SEEDS = 488;
        private const int ARTICHOKE_SEEDS = 489;
        private const int PUMPKIN_SEEDS = 490;
        private const int BOK_CHOY_SEEDS = 491;
        private const int YAM_SEEDS = 492;
        private const int CRANBERRY_SEEDS = 493;
        private const int BEET_SEEDS = 494;
        private const int SPRING_SEEDS = 495;
        private const int SUMMER_SEEDS = 496;
        private const int FALL_SEEDS = 497;
        private const int WINTER_SEEDS = 498;
        private const int ANCIENT_SEEDS = 499;
        private const int STRAWBERRY_SEEDS = 745;
        private const int MIXED_SEEDS = 770;
        private const int CACTUS_SEEDS = 802;
        private const int PINEAPPLE_SEEDS = 833;
        private const int FIBER_SEEDS = 885;

        private const int BASIC_FERTILIZER = 368;
        private const int QUALITY_FERTILIZER = 369;
        private const int DELUXE_FERTILIZER = 919;

        private const int BASIC_RETAINING_SOIL = 370;
        private const int QUALITY_RETAINING_SOIL = 371;
        private const int DELUXE_RETAINING_SOIL = 920;

        private const int SPEED_GRO = 465;
        private const int DELUXE_SPEED_GRO = 466;
        private const int HYPER_SPEED_GRO = 918;

        private const int BANANA_SAPLING = 69;
        private const int CHERRY_SAPLING = 628;
        private const int APRICOT_SAPLING = 629;
        private const int ORANGE_SAPLING = 630;
        private const int PEACH_SAPLING = 631;
        private const int POMEGRANATE_SAPLING = 632;
        private const int APPLE_SAPLING = 633;
        private const int MANGO_SAPLING = 835;

        private const int BOUQUET = 458;
    }
}
