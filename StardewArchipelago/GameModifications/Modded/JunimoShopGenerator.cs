using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.GameModifications.Modded
{
    public class JunimoShopGenerator
    {
        private ShopStockGenerator _shopStockGenerator;
        private StardewItemManager _stardewItemManager;
        private PersistentStock BluePersistentStock { get; }
        private PersistentStock GreyPersistentStock { get; }
        private PersistentStock YellowPersistentStock { get; }
        private PersistentStock RedPersistentStock { get; }
        private static Dictionary<string, JunimoVendor> JunimoVendors { get; set; }
        private Dictionary<int, int> BlueItems { get; set; }
        private static readonly List<string> BlueColors = new()
        {
            "color_blue", "color_aquamarine", "color_dark_blue", "color_cyan", "color_light_cyan"
        };
        private Dictionary<int, int> GreyItems { get; set; }
        private static readonly List<string> GreyColors = new()
        {
            "color_gray", "color_black", "color_poppyseed", "color_dark_gray"
        };
        private Dictionary<int, int> RedItems { get; set; }
        private static readonly List<string> RedColors = new()
        {
            "color_red", "color_pink", "color_dark_pink"
        };
        private Dictionary<int, int> YellowItems { get; set; }
        private static readonly List<string> YellowColors = new()
        {
            "color_yellow", "color_gold", "color_sand"
        };
        private static readonly string[] spring = new string[]{"spring"};
        private static readonly string[] summer = new string[]{"summer"};
        private static readonly string[] fall = new string[]{"fall"};

        private class JunimoVendor
        {
            public PersistentStock JunimoPersistentStock {get; private set;}
            public Dictionary<int, int> ColorItems {get; private set;}

            public JunimoVendor(PersistentStock junimoPersistentStock, Dictionary<int, int> colorItems)
            {
                JunimoPersistentStock = junimoPersistentStock;
                ColorItems = colorItems;
            }
        }

        public JunimoShopGenerator(ShopStockGenerator shopStockGenerator, StardewItemManager stardewItemManager)
        {
            _shopStockGenerator = shopStockGenerator;
            _stardewItemManager = stardewItemManager;
            RedPersistentStock = new PersistentStock();
            GreyPersistentStock = new PersistentStock();
            BluePersistentStock = new PersistentStock();
            YellowPersistentStock = new PersistentStock();
            GenerateColoredItems();
            var blueVendor = new JunimoVendor(BluePersistentStock, BlueItems);
            var yellowVendor = new JunimoVendor(YellowPersistentStock, YellowItems);
            var greyVendor = new JunimoVendor(GreyPersistentStock, GreyItems);
            var redVendor = new JunimoVendor(RedPersistentStock, RedItems);
            JunimoVendors = new Dictionary<string, JunimoVendor>(){
                {"Blue", blueVendor}, {"Yellow", yellowVendor}, {"Grey", greyVendor}, {"Red", redVendor}
            };
        }

        public void GenerateColoredItems()
        {
            BlueItems = new Dictionary<int, int>();
            YellowItems = new Dictionary<int, int>();
            GreyItems = new Dictionary<int, int>();
            RedItems = new Dictionary<int, int>();
            var objectContextTags = Game1.content.Load<Dictionary<string, string>>("Data\\ObjectContextTags");

            foreach (var contextItem in objectContextTags)
            {
                var itemContext = contextItem.Value;
                if (!_stardewItemManager.ItemExists(contextItem.Key))
                    continue;
                var item = _stardewItemManager.GetItemByName(contextItem.Key);
                if (item.SellPrice == 1)
                    continue;
                if (IsColor(itemContext, "blue") && !itemContext.Contains("fish") && !itemContext.Contains("marine"))
                {
                    
                    BlueItems[item.Id] = item.SellPrice;
                    continue;
                }
                if (IsColor(itemContext, "yellow"))
                {
                    YellowItems[item.Id] = item.SellPrice;
                    continue;
                }
                if (IsColor(itemContext, "grey"))
                {
                    GreyItems[item.Id] = item.SellPrice;
                }
                if (IsColor(itemContext, "red"))
                {
                    RedItems[item.Id] = item.SellPrice;
                }
            }
        }

        private static bool IsColor(string itemContext, string color)
        {
            if (color == "blue")
            {
                return BlueColors.Any(x => itemContext.Contains(x));
            }
            if (color == "yellow")
            {
                return YellowColors.Any(x => itemContext.Contains(x));
            }
            if (color == "red")
            {
                return RedColors.Any(x => itemContext.Contains(x));
            }
            if (color == "grey")
            {
                return GreyColors.Any(x => itemContext.Contains(x));
            }
            return false;
        }

        public Dictionary<ISalable, int[]> GetJunimoShopStock(string color)
        {
            var stockAlreadyExists = JunimoVendors[color].JunimoPersistentStock.TryGetStockForToday(out var stock);
            if (!stockAlreadyExists)
            {
                stock = GenerateJunimoStock(color);
                JunimoVendors[color].JunimoPersistentStock.SetStockForToday(stock);
            }

            return stock;
        }

        private Dictionary<ISalable, int[]> GenerateJunimoStock(string color)
        {
            var stock = new Dictionary<ISalable, int[]>();
            if (color == "Blue")
            {
                stock = GenerateBlueJunimoStock(stock);
            }
            if (color == "Red")
            {
                stock = GenerateMuseumItemsForJunimoStock("Red", stock);
            }
            if (color == "Grey")
            {
                stock = GenerateMuseumItemsForJunimoStock("Grey", stock);
            }
            if (color == "Yellow")
            {
                AddSeedsToYellowStock(stock);
            }
            return stock;
        }

        private static void AddToJunimoStock(
            Dictionary<ISalable, int[]> stock,
            int itemId,
            string color,
            string[] itemSeason = null)
        {
            var item = new StardewValley.Object(Vector2.Zero, itemId, 1);

            if (itemSeason != null)
            {
                if (!itemSeason.Contains(Game1.currentSeason))
                {
                    return;
                }
            }

            var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + itemId);
            if (random.NextDouble() < 0.4)
            {
                return;
            }
            var itemToTrade = JunimoVendors[color].ColorItems.ElementAt(random.Next(JunimoVendors[color].ColorItems.Count));
            var itemToTradeTotal = ExchangeRate(item, itemToTrade.Value);

            if (Game1.player.hasCompletedCommunityCenter())
            {
                itemToTradeTotal[1] = Math.Min(1, 4* itemToTradeTotal[1] / 5);
            }
            item.Stack = itemToTradeTotal[0];
            
            stock.Add(item, new int[4]
            {
                0,
                int.MaxValue,
                itemToTrade.Key,
                itemToTradeTotal[1],
            });
        }

        // Lets just say they no currency good cuz Junimo
        private static int[] ExchangeRate(Item soldItem, int wantedItem)
        {
            if (soldItem.salePrice() > wantedItem && soldItem.salePrice() % wantedItem == 0)
                return new int[2]{1, soldItem.salePrice() / wantedItem};
            if (soldItem.salePrice() <= wantedItem &&  wantedItem % soldItem.salePrice() == 0)
                return new int[2]{wantedItem / soldItem.salePrice(), 1};
            var gcd = Gcd(soldItem.salePrice(), wantedItem);
            var lcm = soldItem.salePrice() * wantedItem / gcd;
            var requestCount = lcm/soldItem.salePrice();
            var offerCount = lcm/wantedItem;
            var lowestTrade = 5; // This is for us to change if we want to move this value around easily in testing
            if (Math.Min(requestCount, offerCount) > lowestTrade)
            {
                var closestTen = (int) Math.Pow(lowestTrade, (int) ( Math.Log10(Math.Min(requestCount, offerCount)) / Math.Log10(lowestTrade) ) );
                requestCount /= closestTen;
                offerCount /= closestTen;
                requestCount /= Gcd(requestCount, offerCount); // Due to the rounding we may find the two aren't relatively prime anymore
                offerCount /= Gcd(requestCount, offerCount);
            }
            
            return new int[2]{requestCount, offerCount};
        }

        private static Dictionary<ISalable, int[]> GenerateBlueJunimoStock(Dictionary<ISalable, int[]> stock)
        {
            var fishData = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
            foreach (var fish in Game1.player.fishCaught.Keys)
            {
                string[] fishSeasons = null;
                if (!fishData.ContainsKey(fish))
                {
                    continue; // Some things you fish up aren't fish; ignore em
                }
                if (fish == 153 || fish == 157 || fish == 152) // We algae haters keep scrollin
                    continue;
                if (fishData[fish].Split("/")[1] != "trap")
                {
                    fishSeasons = fishData[fish].Split("/")[6].Split(" ");
                }
                AddToJunimoStock(stock, fish, "Blue", fishSeasons);
            }
            return stock;
        }

        private static Dictionary<ISalable, int[]> GenerateMuseumItemsForJunimoStock(string color, Dictionary<ISalable, int[]> stock)
        {
            var type = "";
            if (color == "Grey")
            {
                type = "Mineral";
            }
            else
            {
                type = "Arch";
            }
            var libraryMuseum = Game1.getLocationFromName("ArchaeologyHouse") as  LibraryMuseum;
            var museumItems = new HashSet<int>(libraryMuseum.museumPieces.Values);
            var objectInformation = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            foreach (var museumitemId in museumItems)
            {
                var itemType = objectInformation[museumitemId].Split("/")[3];
                if (!itemType.Contains(type))
                {
                    continue;
                }
                AddToJunimoStock(stock, museumitemId, color);
            }
            return stock;
        }

        private static int Gcd(int val1, int val2)
        {
            var a = Math.Max(val1, val2);
            var b = Math.Min(val1, val2);
            var r = a % b;
            if ( r == 0)
            {
                if (val1 > val2)
                {
                    return val2;
                }
                else
                {
                    return val1;
                }
            }
            while (r != 0)
            {
                a = b;
                b = r;
                if (a % b == 0)
                    break;
                r = a % b;
            }
            return r;
        }

        private static void AddSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            AddSpringSeedsToYellowStock(stock);
            AddSummerSeedsToYellowStock(stock);
            AddFallSeedsToYellowStock(stock);
        }

        private static void AddSpringSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJunimoStock(stock, ShopItemIds.PARSNIP_SEEDS, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.BEAN_STARTER, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.CAULIFLOWER_SEEDS, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.POTATO_SEEDS, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.TULIP_BULB, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.KALE_SEEDS, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.JAZZ_SEEDS, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.GARLIC_SEEDS, "Yellow", spring);
            AddToJunimoStock(stock, ShopItemIds.RICE_SHOOT, "Yellow", spring);
        }

        private static void AddSummerSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJunimoStock(stock, ShopItemIds.MELON_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.TOMATO_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.BLUEBERRY_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.PEPPER_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.WHEAT_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.RADISH_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.POPPY_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.SPANGLE_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.HOPS_STARTER, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.CORN_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.SUNFLOWER_SEEDS, "Yellow", summer);
            AddToJunimoStock(stock, ShopItemIds.RED_CABBAGE_SEEDS, "Yellow", summer);
        }

        private static void AddFallSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            AddToJunimoStock(stock, ShopItemIds.PUMPKIN_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.CORN_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.EGGPLANT_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.BOK_CHOY_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.YAM_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.CRANBERRY_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.WHEAT_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.SUNFLOWER_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.FAIRY_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.AMARANTH_SEEDS, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.GRAPE_STARTER, "Yellow", fall);
            AddToJunimoStock(stock, ShopItemIds.ARTICHOKE_SEEDS, "Yellow", fall);
        }
                private static void AddSaplingsToShop(Dictionary<ISalable, int[]> stock)
        {
            AddToJunimoStock(stock, ShopItemIds.CHERRY_SAPLING, "Yellow");
            AddToJunimoStock(stock, ShopItemIds.APRICOT_SAPLING, "Yellow");
            AddToJunimoStock(stock, ShopItemIds.ORANGE_SAPLING, "Yellow");
            AddToJunimoStock(stock, ShopItemIds.PEACH_SAPLING, "Yellow");
            AddToJunimoStock(stock, ShopItemIds.POMEGRANATE_SAPLING, "Yellow");
            AddToJunimoStock(stock, ShopItemIds.APPLE_SAPLING, "Yellow");
        }

    }
}