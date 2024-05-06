using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.GameModifications.Modded
{
    public class JunimoShopGenerator
    {
        private ArchipelagoClient _archipelago;
        private SeedShopStockModifier _seedShopStockModifier;
        private StardewItemManager _stardewItemManager;

        // Persistent Stock no longer exists, because the new modding API stuff allows for making a persistent stock using a single parameter
        // Furthermore, it also allows for very easy and customizable Bartering, so I think like 99% of this file will be gone once you're done with it
        // I'm commenting it all out for now

        //private PersistentStock BluePersistentStock { get; }
        //private PersistentStock GreyPersistentStock { get; }
        //private PersistentStock YellowPersistentStock { get; }
        //private PersistentStock RedPersistentStock { get; }
        //private PersistentStock OrangePersistentStock { get; }
        //private static Dictionary<string, JunimoVendor> JunimoVendors { get; set; }
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

        //private class JunimoVendor
        //{
        //    public PersistentStock JunimoPersistentStock { get; private set; }
        //    public Dictionary<int, int> ColorItems { get; private set; }

        //    public JunimoVendor(PersistentStock junimoPersistentStock, Dictionary<int, int> colorItems)
        //    {
        //        JunimoPersistentStock = junimoPersistentStock;
        //        ColorItems = colorItems;
        //    }
        //}

        public JunimoShopGenerator(ArchipelagoClient archipelago, SeedShopStockModifier seedShopStockModifier, StardewItemManager stardewItemManager)
        {
            throw new Exception($"{nameof(JunimoShopGenerator)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //_archipelago = archipelago;
            //_seedShopStockModifier = seedShopStockModifier;
            //_stardewItemManager = stardewItemManager;
            //RedPersistentStock = new PersistentStock();
            //GreyPersistentStock = new PersistentStock();
            //BluePersistentStock = new PersistentStock();
            //YellowPersistentStock = new PersistentStock();
            //OrangePersistentStock = new PersistentStock();
            //GenerateItems();
            //var blueVendor = new JunimoVendor(BluePersistentStock, BlueItems);
            //var yellowVendor = new JunimoVendor(YellowPersistentStock, YellowItems);
            //var greyVendor = new JunimoVendor(GreyPersistentStock, GreyItems);
            //var redVendor = new JunimoVendor(RedPersistentStock, RedItems);
            //var orangeVendor = new JunimoVendor(OrangePersistentStock, OrangeItems);
            //JunimoVendors = new Dictionary<string, JunimoVendor>(){
            //    {"Blue", blueVendor}, {"Yellow", yellowVendor}, {"Grey", greyVendor}, {"Red", redVendor}, {"Orange", orangeVendor},
            //};
        }

        public void GenerateItems()
        {
            throw new Exception($"{nameof(GenerateItems)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //BlueItems = new Dictionary<int, int>();
            //YellowItems = new Dictionary<int, int>();
            //GreyItems = new Dictionary<int, int>();
            //RedItems = new Dictionary<int, int>();
            //OrangeItems = new Dictionary<int, int>();
            //PurpleItems = new Dictionary<int, int>();
            //BerryItems = new Dictionary<StardewItem, int>();
            //var objectContextTags = Game1.objectContextTags;
            //var objectInformation = Game1.objectInformation;

            //foreach (var contextItem in objectContextTags)
            //{
            //    var itemContext = contextItem.Value;
            //    if (!_stardewItemManager.ItemExists(contextItem.Key))
            //        continue;
            //    var item = _stardewItemManager.GetItemByName(contextItem.Key);
            //    var type = objectInformation[item.Id].Split("/")[3];
            //    if (item.SellPrice <= 1)
            //        continue;
            //    if ((item.Name.Contains("Berry") || item.Name.Contains("berry")) && !item.Name.Contains("Joja") && !itemContext.Contains("cooking") && !itemContext.Contains("Seeds"))
            //    {
            //        BerryItems[item] = item.SellPrice;
            //    }
            //    if (IsColor(itemContext, "blue") && !itemContext.Contains("fish") && !itemContext.Contains("marine"))
            //    {
            //        BlueItems[item.Id] = item.SellPrice;
            //        continue;
            //    }
            //    if (IsColor(itemContext, "yellow"))
            //    {
            //        YellowItems[item.Id] = item.SellPrice;
            //        continue;
            //    }
            //    if (IsColor(itemContext, "grey") && !type.Contains("Minerals"))
            //    {
            //        GreyItems[item.Id] = item.SellPrice;
            //        continue;
            //    }
            //    if (IsColor(itemContext, "red") && !type.Contains("Arch"))
            //    {
            //        RedItems[item.Id] = item.SellPrice;
            //        continue;
            //    }
            //    if (IsColor(itemContext, "orange") && !itemContext.Contains("cooking"))
            //    {
            //        OrangeItems[item.Id] = item.SellPrice;
            //        continue;
            //    }
            //    if (IsColor(itemContext, "purple") && type.Contains("Basic"))
            //    {
            //        PurpleItems[item.Id] = item.SellPrice;
            //        continue;
            //    }
            //}
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
            if (color == "orange")
            {
                return OrangeColors.Any(x => itemContext.Contains(x));
            }
            if (color == "purple")
            {
                return PurpleColors.Any(x => itemContext.Contains(x));
            }
            return false;
        }

        public Dictionary<ISalable, int[]> GetJunimoShopStock(string color, Dictionary<ISalable, int[]> oldStock)
        {
            throw new Exception($"{nameof(GetJunimoShopStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //var stockAlreadyExists = JunimoVendors[color].JunimoPersistentStock.TryGetStockForToday(out var stock);
            //if (!stockAlreadyExists)
            //{
            //    stock = GenerateJunimoStock(color, oldStock);
            //    JunimoVendors[color].JunimoPersistentStock.SetStockForToday(stock);
            //}

            //return stock;
        }

        private Dictionary<ISalable, int[]> GenerateJunimoStock(string color, Dictionary<ISalable, int[]> oldStock)
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
            if (color == "Orange")
            {
                GenerateOrangeJunimoStock(stock, oldStock);
            }
            return stock;
        }

        private Dictionary<ISalable, int[]> GenerateBlueJunimoStock(Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(GenerateBlueJunimoStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //var fishData = DataLoader.Fish(Game1.content);
            //foreach (var fish in Game1.player.fishCaught.Keys)
            //{
            //    string[] fishSeasons = null;
            //    if (!fishData.ContainsKey(fish))
            //    {
            //        continue; // Some things you fish up aren't fish; ignore em
            //    }
            //    if (fish == 153 || fish == 157 || fish == 152) // We algae haters keep scrollin
            //        continue;
            //    if (fishData[fish].Split("/")[1] != "trap")
            //    {
            //        fishSeasons = fishData[fish].Split("/")[6].Split(" ");
            //    }
            //    AddToJunimoStock(stock, fish, "Blue", false, fishSeasons);
            //}
            //return stock;
        }

        private Dictionary<ISalable, int[]> GenerateMuseumItemsForJunimoStock(string color, Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(GenerateMuseumItemsForJunimoStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //var isGrey = color == "Grey";
            //var artifactsFound = Game1.player.archaeologyFound.Keys;
            //var mineralsFound = Game1.player.mineralsFound.Keys;
            //if (isGrey)
            //{
            //    foreach (var museumitemId in mineralsFound)
            //    {
            //        AddToJunimoStock(stock, museumitemId, color, false);
            //    }
            //}
            //else
            //{
            //    foreach (var museumitemId in artifactsFound)
            //    {
            //        if (museumitemId == 102) //No lost books smh get out
            //            continue;
            //        AddToJunimoStock(stock, museumitemId, color, false);
            //    }
            //}

            //return stock;
        }

        private Dictionary<ISalable, int[]> GenerateOrangeJunimoStock(Dictionary<ISalable, int[]> stock, Dictionary<ISalable, int[]> oldStock)
        {
            foreach (var (item, value) in oldStock)
            {
                var shopItem = _stardewItemManager.GetItemByName(item.Name);
                AddToJunimoStock(stock, shopItem, "Orange", "BigCraftable", 0, value[0]);
            }
            return stock;
        }

        private void AddToJunimoStock(
            Dictionary<ISalable, int[]> stock,
            StardewItem stardewItem,
            string color,
            string category = null,
            double failRate = 0.4,
            int uniquePrice = -1)
        {
            throw new Exception($"{nameof(AddToJunimoStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //var itemName = stardewItem.Name;
            //var item = new StardewValley.Object(Vector2.Zero, stardewItem.Id, 1);
            //if (category == "BigCraftable")
            //{
            //    item = new StardewValley.Object(Vector2.Zero, stardewItem.Id);
            //}
            //var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + stardewItem.Id);
            //if (random.NextDouble() < failRate)
            //{
            //    return;
            //}
            //var colorItems = JunimoVendors[color].ColorItems.Keys.ToList();
            //var randomColorItem = colorItems[random.Next(colorItems.Count)];
            //var randomColorValue = 0.8*JunimoVendors[color].ColorItems[randomColorItem];
            //var colorItemExchangeRate = ExchangeRate(Math.Max(uniquePrice, item.salePrice()), (int) randomColorValue);

            //StockListing(item, stock, colorItemExchangeRate[0], randomColorItem, colorItemExchangeRate[1]);
        }

        private static void StockListing(ISalable item, Dictionary<ISalable, int[]> stock, int stackSize, int itemForSaleId, int value)
        {
            item.Stack = stackSize;

            stock[item] = new int[4]
            {
                0,
                int.MaxValue,
                itemForSaleId,
                value,
            };
        }

        /*private void AddToJunimoStock(
            Dictionary<ISalable, int[]> stock,
            string itemName,
            string color,
            int uniquePrice)
        {
            AddToJunimoStock(stock, _stardewItemManager.GetItemByName(itemName), color, null, 0.4, uniquePrice);
        }*/

        private void AddToJunimoStock(
            Dictionary<ISalable, int[]> stock,
            int itemId,
            string color,
            bool isSeed,
            string[] itemSeason = null)
        {
            throw new Exception($"{nameof(AddToJunimoStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //var item = _stardewItemManager.GetObjectById(yieldItemId);
            //if (isSeed && _archipelago.SlotData.Cropsanity == Cropsanity.Shuffled && !_archipelago.HasReceivedItem(item.Name))
            //{
            //    return;
            //}
            //if (itemSeason != null && !itemSeason.Contains(Game1.currentSeason))
            //{
            //    return;
            //}
            //AddToJunimoStock(stock, item, color, null);
        }

        public int[] ExchangeRate(int soldItemValue, int requestedItemValue)
        {
            if (IsOnePriceAMultipleOfOther(soldItemValue, requestedItemValue, out var exchangeRate))
            {
                return exchangeRate;
            }
            var greatestCommonDivisor = GreatestCommonDivisor(soldItemValue, requestedItemValue);
            var leastCommonMultiple = soldItemValue * requestedItemValue / greatestCommonDivisor;
            var soldItemCount = leastCommonMultiple / soldItemValue;
            var requestedItemCount = leastCommonMultiple / requestedItemValue;

            var applesDiscount = GiveApplesFriendshipDiscount(soldItemCount, requestedItemCount);
            soldItemCount = applesDiscount[0];
            requestedItemCount = applesDiscount[1];

            var lowestCount = 5; // This is for us to change if we want to move this value around easily in testing
            var finalCounts = MakeMinimalCountBelowGivenCount(soldItemCount, requestedItemCount, lowestCount);
            return finalCounts;
        }

        private bool IsOnePriceAMultipleOfOther(int soldItemValue, int requestedItemValue, out int[] exchangeRate)
        {
            exchangeRate = null;
            if (soldItemValue > requestedItemValue && soldItemValue % requestedItemValue == 0)
            {
                exchangeRate = new int[2] { 1, soldItemValue / requestedItemValue };
                return true;
            }
            if (soldItemValue <= requestedItemValue && requestedItemValue % soldItemValue == 0)
            {
                exchangeRate = new int[2] { requestedItemValue / soldItemValue, 1 };
                return true;
            }

            return false;
        }

        private int[] GiveApplesFriendshipDiscount(int soldItemCount, int requestedItemCount)
        {
            var applesHearts = 0;
            if (Game1.player.friendshipData.ContainsKey("Apples"))
            {
                applesHearts = Game1.player.friendshipData["Apples"].Points / 250; // Get discount from being friends with Apples
            }
            if (requestedItemCount == 1)
            {
                soldItemCount = (int)(soldItemCount * (1 + applesHearts * 0.05f));
            }
            else
            {
                requestedItemCount = (int)Math.Max(1, requestedItemCount * (1 - applesHearts * 0.05f));
            }
            return new int[2] { soldItemCount, requestedItemCount };

        }

        private int[] MakeMinimalCountBelowGivenCount(int soldItemCount, int requestedItemCount, int givenCount)
        {
            if (Math.Min(soldItemCount, requestedItemCount) > givenCount)
            {
                var closestCount = (int)Math.Pow(givenCount, (int)(Math.Log10(Math.Min(soldItemCount, requestedItemCount)) / Math.Log10(givenCount)));
                soldItemCount /= closestCount;
                requestedItemCount /= closestCount;
                var greatestCommonDivisor = GreatestCommonDivisor(soldItemCount, requestedItemCount); // Due to the rounding we may find the two aren't relatively prime anymore
                soldItemCount /= greatestCommonDivisor;
                requestedItemCount /= greatestCommonDivisor;
            }
            return new int[2] { soldItemCount, requestedItemCount };
        }

        private void AddSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(AddSeedsToYellowStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //AddSpringSeedsToYellowStock(stock);
            //AddSummerSeedsToYellowStock(stock);
            //AddFallSeedsToYellowStock(stock);
            //AddSaplingsToShop(stock);
            //AddToJunimoStock(stock, QualifiedItemIds.RHUBARB_SEEDS, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.STARFRUIT_SEEDS, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.BEET_SEEDS, "Yellow", true);
            //AddJunimoModdedStock(stock);
        }

        private void AddSpringSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(AddSpringSeedsToYellowStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //AddToJunimoStock(stock, QualifiedItemIds.PARSNIP_SEEDS, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.BEAN_STARTER, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.CAULIFLOWER_SEEDS, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.POTATO_SEEDS, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.TULIP_BULB, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.KALE_SEEDS, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.JAZZ_SEEDS, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.GARLIC_SEEDS, "Yellow", true, spring);
            //AddToJunimoStock(stock, QualifiedItemIds.RICE_SHOOT, "Yellow", true, spring);
        }

        private void AddSummerSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(AddSummerSeedsToYellowStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //AddToJunimoStock(stock, QualifiedItemIds.MELON_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.TOMATO_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.BLUEBERRY_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.PEPPER_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.WHEAT_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.RADISH_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.POPPY_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.SPANGLE_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.HOPS_STARTER, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.CORN_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.SUNFLOWER_SEEDS, "Yellow", true, summer);
            //AddToJunimoStock(stock, QualifiedItemIds.RED_CABBAGE_SEEDS, "Yellow", true, summer);
        }

        private void AddFallSeedsToYellowStock(Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(AddFallSeedsToYellowStock)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //AddToJunimoStock(stock, QualifiedItemIds.PUMPKIN_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.CORN_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.EGGPLANT_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.BOK_CHOY_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.YAM_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.CRANBERRY_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.WHEAT_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.SUNFLOWER_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.FAIRY_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.AMARANTH_SEEDS, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.GRAPE_STARTER, "Yellow", true, fall);
            //AddToJunimoStock(stock, QualifiedItemIds.ARTICHOKE_SEEDS, "Yellow", true, fall);
        }

        private void AddSaplingsToShop(Dictionary<ISalable, int[]> stock)
        {
            throw new Exception($"{nameof(AddSaplingsToShop)} is not ready for 1.6; Look at my comment at the beginning of the file");
            //AddToJunimoStock(stock, QualifiedItemIds.CHERRY_SAPLING, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.APRICOT_SAPLING, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.ORANGE_SAPLING, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.PEACH_SAPLING, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.POMEGRANATE_SAPLING, "Yellow", true);
            //AddToJunimoStock(stock, QualifiedItemIds.APPLE_SAPLING, "Yellow", true);
        }

        private void AddJunimoModdedStock(Dictionary<ISalable, int[]> stock)
        {
        }


        public int ValueOfOneItemWithWeight(int[] offerRatio, double weight)
        {
            return (int)Math.Pow(offerRatio[1] / offerRatio[0], weight);
        }

        private static int GreatestCommonDivisor(int firstValue, int secondValue) //Seemingly no basic method outside of BigInteger?
        {
            var largestValue = Math.Max(firstValue, secondValue);
            var lowestValue = Math.Min(firstValue, secondValue);
            var remainder = largestValue % lowestValue;
            if (remainder == 0)
            {
                return lowestValue;
            }
            while (remainder != 0)
            {
                largestValue = lowestValue;
                lowestValue = remainder;
                if (largestValue % lowestValue == 0)
                    break;
                remainder = largestValue % lowestValue;
            }
            return remainder;
        }
    }
}
