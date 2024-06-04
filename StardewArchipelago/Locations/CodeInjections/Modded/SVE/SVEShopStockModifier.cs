using System;
using System.Collections.Generic;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class SVEShopStockModifier : ShopStockModifier
    {
        private static new ArchipelagoClient _archipelago;
        private static new StardewItemManager _stardewItemManager;
        private static readonly string SALMONBERRY_ID = "296";
        private static readonly string BLACKBERRY_ID = "410";
        private static readonly string SPICEBERRY_ID = "396";
        private static readonly string CRYSTALFRUIT_ID = "414";
        public SVEShopStockModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(monitor, helper, archipelago, stardewItemManager)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
        }

        /*private static readonly Dictionary<string, Item> seasonalBerry = new()
        {
            { "spring", new Object("296", 1) },
            { "summer", new Object("396", 1) },
            { "fall", new Object("410", 1) },
            { "winter", new Object("414", 1) },
        };*/

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;
                    var alesiaShop = shopsData["FlashShifter.StardewValleyExpandedCP_AlesiaVendor"];
                    var isaacShop = shopsData["FlashShifter.StardewValleyExpandedCP_IsaacVendor"];
                    var bearShop = shopsData["FlashShifter.StardewValleyExpandedCP_BearVendor"];
                    ReplaceAlesiaShopWithChecks(alesiaShop);
                    ReplaceIsaacWeaponsWithChecks(isaacShop);
                    MakeBearBarter(bearShop);
                },
                AssetEditPriority.Late
            );
        }

        private void ReplaceAlesiaShopWithChecks(ShopData shopData)
        {
            const string daggerLocationName = "Tempered Galaxy Dagger";
            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!item.Id.Equals(ModItemIds.TEMPERED_DAGGER, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var apShopItem = CreateArchipelagoLocation(item, daggerLocationName);
                shopData.Items.RemoveAt(i);
                shopData.Items.Insert(i, apShopItem);
            }
        }

        private void ReplaceIsaacWeaponsWithChecks(ShopData shopData)
        {
            const string swordLocationName = "Tempered Galaxy Sword";
            const string hammerLocationName = "Tempered Galaxy Hammer";
            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (item.Id.Equals(ModItemIds.TEMPERED_SWORD, StringComparison.InvariantCultureIgnoreCase))
                {
                    var apShopItem = CreateArchipelagoLocation(item, swordLocationName);
                    shopData.Items.RemoveAt(i);
                    shopData.Items.Insert(i, apShopItem);
                }
                else if (item.Id.Equals(ModItemIds.TEMPERED_HAMMER, StringComparison.InvariantCultureIgnoreCase))
                {
                    var apShopItem = CreateArchipelagoLocation(item, hammerLocationName);
                    shopData.Items.RemoveAt(i);
                    shopData.Items.Insert(i, apShopItem);
                }
            }
        }

        private void MakeBearBarter(ShopData shopData)
        {
            var berryItems = _stardewItemManager.GetObjectsWithPhrase("berry");
            var newItems = new List<ShopItemData>();
            foreach (var item in shopData.Items)
            {
                var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + item.GetHashCode());
                var chosenItemGroup = berryItems.FindAll(x => !(x.Name.Contains("Joja") || x.Name.Contains("Seeds")) && x.SellPrice != 0 );
                var chosenItem = chosenItemGroup[random.Next(chosenItemGroup.Count)];
                var isRecipe = item.ItemId.Contains("Baked Berry Oatmeal") || item.ItemId.Contains("Flower Cookie");
                if (isRecipe && _archipelago.SlotData.Chefsanity.HasFlag(Chefsanity.Purchases))
                {
                    chosenItem = BerryIfChefsanityIsOn();
                }
                newItems.Add(SwapItemToBerryBarter(item, chosenItem));
            }
            shopData.Items = newItems;
        }

        private StardewObject BerryIfChefsanityIsOn()
        {
            if (Game1.season.HasFlag(Season.Spring))
            {
                return _stardewItemManager.GetObjectById(SALMONBERRY_ID);
            }
            else if (Game1.season.HasFlag(Season.Summer))
            {
                return _stardewItemManager.GetObjectById(SPICEBERRY_ID);
            }
            else if (Game1.season.HasFlag(Season.Fall))
            {
                return _stardewItemManager.GetObjectById(BLACKBERRY_ID);
            }
            return _stardewItemManager.GetObjectById(CRYSTALFRUIT_ID);
        }

        protected ShopItemData SwapItemToBerryBarter(ShopItemData item, StardewObject berryItem)
        {
            var hasKnowledge = _archipelago.HasReceivedItem("Bear Knowledge");
            var bearShopItem = item.DeepClone();
            bearShopItem.Id = $"{ModEntry.Instance.ModManifest.UniqueID}_{berryItem.Name.Replace(" ", "_")}";
            var divisorBonus = hasKnowledge ? 10 : 5;
            var chosenItemExchangeRate = ExchangeRate(item.Price / divisorBonus, berryItem.SellPrice);
            bearShopItem.TradeItemId = berryItem.Id;
            bearShopItem.Price = 0;
            bearShopItem.MinStack = chosenItemExchangeRate[0];
            bearShopItem.TradeItemAmount = chosenItemExchangeRate[1];
            return bearShopItem;
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

