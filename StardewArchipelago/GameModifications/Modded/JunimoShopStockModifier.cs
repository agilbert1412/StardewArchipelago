using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications.Modded
{
    public class JunimoShopStockModifier : ShopStockModifier
    {
        private static readonly string[] spring = new string[]{"spring"};
        private static readonly string[] summer = new string[]{"summer"};
        private static readonly string[] fall = new string[]{"fall"};
        private static readonly string[] winter = new string[]{"winter"};

        private static readonly Dictionary<string, string> _junimoPhrase = new()
        {
            { "Orange", "Look! Me gib pretty for orange thing!" },
            { "Red", "Give old things for red gubbins!" },
            { "Grey", "I trade rocks for grey what's-its!" },
            { "Yellow", "I hab seeds, gib yellow gubbins!" },
            { "Blue", "I hab fish! You give blue pretty?" },
            { "Purple", "Rare thing?  Purple thing!  Yay!"}
        };

        public JunimoShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(monitor, helper, archipelago, stardewItemManager)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;
                    var orangeShop = shopsData["FlashShifter.StardewValleyExpandedCP_OrangeJunimoVendor"];
                    var purpleShop = shopsData["FlashShifter.StardewValleyExpandedCP_PurpleJunimoVendor"];
                    var redShop = shopsData["FlashShifter.StardewValleyExpandedCP_RedJunimoVendor"];
                    var blueShop = shopsData["FlashShifter.StardewValleyExpandedCP_BlueJunimoVendor"];
                    var greyShop = shopsData["FlashShifter.StardewValleyExpandedCP_GreyJunimoVendor"];
                    var yellowShop = shopsData["FlashShifter.StardewValleyExpandedCP_YellowJunimoVendor"];
                    GenerateBlueJunimoStock(blueShop);
                    GenerateRedJunimoStock(redShop);
                    GenerateGreyJunimoStock(greyShop);
                    GenerateYellowJunimoStock(yellowShop);
                    GenerateOrangeJunimoStock(orangeShop);
                    GeneratePurpleJunimoStock(purpleShop);
                },
                AssetEditPriority.Late
            );
        }
        
        private void GenerateBlueJunimoStock(ShopData shopData)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Blue"];
            var fishData = DataLoader.Fish(Game1.content);
            shopData.Items.Clear();

            foreach (var fish in fishData)
            {
                string[] fishSeasons = null;
                if (fishData[fish.Key].Split("/")[1] != "trap")
                {
                    fishSeasons = fishData[fish.Key].Split("/")[6].Split(" ");
                }
                var fishItem = _stardewItemManager.GetObjectById(fish.Key);
                var completeCondition = fishSeasons is not null ? 
                $"{GameStateConditionProvider.CreateSeasonsCondition(fishSeasons)}, RANDOM 0.4 @addDailyLuck, PLAYER_HAS_CAUGHT_FISH Current {fish.Key}" : 
                "RANDOM 0.4 @addDailyLuck, PLAYER_HAS_CAUGHT_FISH Current {fish.Key}";
                shopData.Items.Add(CreateJunimoShopItem("Blue", fishItem, completeCondition));
            }
        }

        private void GenerateGreyJunimoStock(ShopData shopData)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Grey"];
            var mineralsFound = Game1.player.mineralsFound.Keys;
            shopData.Items.Clear();
            foreach (var rockId in mineralsFound)
            {
                var mineralItem = _stardewItemManager.GetItemByQualifiedId(QualifiedItemIds.QualifiedObjectId(rockId));
                if (rockId == "102") //No lost books smh get out
                    continue;
                shopData.Items.Add(CreateJunimoShopItem("Grey", mineralItem));
            }
        }

        private void GenerateRedJunimoStock(ShopData shopData)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Red"];
            var artifactsFound = Game1.player.archaeologyFound.Keys;
            shopData.Items.Clear();
            foreach (var archId in artifactsFound)
            {
                var archaeologyItem = _stardewItemManager.GetItemByQualifiedId(QualifiedItemIds.QualifiedObjectId(archId));
                if (archId == "102") //No lost books smh get out
                    continue;
                shopData.Items.Add(CreateJunimoShopItem("Red", archaeologyItem));
            }
        }

        private void GenerateOrangeJunimoStock(ShopData shopData)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Orange"];
            foreach (var item in shopData.Items)
            {
                SwapJunimoShopItemWithColor("Orange", item);
            }
        }

        private void GeneratePurpleJunimoStock(ShopData shopData)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Purple"];
            var itemToKeep = shopData.Items.Where(x => x.ItemId == "FlashShifter.StardewValleyExpandedCP_Super_Starfruit").ToList()[0];
            shopData.Items.Clear();
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectById(itemToKeep.Id), itemToKeep.Condition));
            itemToKeep.Condition = null; // The condition is purely for the super starfruit item.  We don't want it on anything else.
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectByName("Magic Rock Candy")));
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectByName("Dewdrop Berry"), null, 4000));
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectByName("Qi Gem"), null, 10000));
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectByName("Calico Egg"), null, 500));
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectByName("Hardwood"), null, 500));
            shopData.Items.Add(CreateJunimoShopItem("Purple", _stardewItemManager.GetObjectByName("Tea Set"), null, 100000));
        }

        private void GenerateYellowJunimoStock(ShopData shopData)
        {
            shopData.Owners[0].Dialogues[0].Dialogue = _junimoPhrase["Yellow"];
            shopData.Items.Clear();
            AddSpringSeedsToYellowStock(shopData);
            AddSummerSeedsToYellowStock(shopData);
            AddFallSeedsToYellowStock(shopData);
            AddSaplingsToShop(shopData);
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.RHUBARB_SEEDS));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.STARFRUIT_SEEDS));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.BEET_SEEDS));
        }

        protected ShopItemData CreateJunimoShopItem(string color, StardewItem stardewItem, string condition = null, int overridePrice = 0)
        {
            var junimoShopItem = new ShopItemData();
            var itemPrice = overridePrice == 0 ? stardewItem.SellPrice : overridePrice;
            junimoShopItem.Id = $"{ModEntry.Instance.ModManifest.UniqueID}_{stardewItem.Name.Replace(" ", "_")}";
            junimoShopItem.ItemId = stardewItem.Id;
            junimoShopItem.AvailableStock = StockBasedOnApplesFriendship();
            junimoShopItem.IsRecipe = false;
            junimoShopItem.AvoidRepeat = true;
            var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + stardewItem.GetHashCode());
            var chosenItemGroup = _stardewItemManager.GetObjectsByColor(color).Where(x => x.SellPrice != 0).ToList();
            var chosenItem = chosenItemGroup[random.Next(chosenItemGroup.Count)];
            var chosenItemExchangeRate = ExchangeRate(itemPrice, chosenItem.SellPrice);
            junimoShopItem.TradeItemId = chosenItem.Id;
            junimoShopItem.Price = 0;
            junimoShopItem.MinStack = chosenItemExchangeRate[0];
            junimoShopItem.TradeItemAmount = chosenItemExchangeRate[1];

            if (condition is not null)
            {
                junimoShopItem.Condition = condition;
            }

            return junimoShopItem;
        }


        protected void SwapJunimoShopItemWithColor(string color, ShopItemData item)
        {
            var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + item.GetHashCode());
            var chosenItemGroup = _stardewItemManager.GetObjectsByColor(color).Where(x => x.SellPrice != 0).ToList();
            var chosenItem = chosenItemGroup[random.Next(chosenItemGroup.Count)];
            var chosenItemExchangeRate = ExchangeRate(item.Price, chosenItem.SellPrice);
            item.Price = 0;
            item.TradeItemId = chosenItem.Id;
            item.TradeItemAmount = chosenItemExchangeRate[1];
            item.MinStack = chosenItemExchangeRate[0];
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

        private int StockBasedOnApplesFriendship()
        {
            var applesHearts = 0;
            if (Game1.player.friendshipData.ContainsKey("Apples"))
            {
                applesHearts = Game1.player.friendshipData["Apples"].Points / 500;
            }
            return applesHearts + 1;
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

        private ShopItemData CreateJunimoSeedItem(string qualifiedId, string[] season = null)
        {
            var seedItemName = _stardewItemManager.GetItemByQualifiedId(qualifiedId).Name;
            var condition = season is not null ? $"{GameStateConditionProvider.CreateSeasonsCondition(season)}, {GameStateConditionProvider.CreateHasReceivedItemCondition(seedItemName)}, RANDOM 0.4 @addDailyLuck" : $"{GameStateConditionProvider.CreateHasReceivedItemCondition(seedItemName)}, RANDOM 0.4 @addDailyLuck";
            return CreateJunimoShopItem("Yellow", _stardewItemManager.GetItemByQualifiedId(qualifiedId), condition);
        }

        private void AddSpringSeedsToYellowStock(ShopData shopData)
        {
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.PARSNIP_SEEDS, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.BEAN_STARTER, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.CAULIFLOWER_SEEDS, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.POTATO_SEEDS, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.TULIP_BULB, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.KALE_SEEDS, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.JAZZ_SEEDS, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.GARLIC_SEEDS, spring));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.RICE_SHOOT, spring));
        }

        private void AddSummerSeedsToYellowStock(ShopData shopData)
        {
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.MELON_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.TOMATO_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.BLUEBERRY_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.PEPPER_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.WHEAT_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.RADISH_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.POPPY_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.SPANGLE_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.HOPS_STARTER, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.CORN_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.SUNFLOWER_SEEDS, summer));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.RED_CABBAGE_SEEDS, summer));
        }

        private void AddFallSeedsToYellowStock(ShopData shopData)
        {
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.PUMPKIN_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.CORN_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.EGGPLANT_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.BOK_CHOY_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.YAM_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.CRANBERRY_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.WHEAT_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.SUNFLOWER_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.FAIRY_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.AMARANTH_SEEDS, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.GRAPE_STARTER, fall));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.ARTICHOKE_SEEDS, fall));
        }

        private void AddSaplingsToShop(ShopData shopData)
        {
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.CHERRY_SAPLING));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.APRICOT_SAPLING));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.ORANGE_SAPLING));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.PEACH_SAPLING));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.POMEGRANATE_SAPLING));
            shopData.Items.Add(CreateJunimoSeedItem(QualifiedItemIds.APPLE_SAPLING));
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