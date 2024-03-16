using System;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;

namespace StardewArchipelago.GameModifications
{
    public class SeedShopStockModifier
    {
        private const float JOJA_PRICE_MULTIPLIER = 0.8f;

        private IMonitor _monitor;
        private IModHelper _modHelper;
        private ArchipelagoClient _archipelago;
        private LocationChecker _locationChecker;

        public SeedShopStockModifier(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public void OnSeedShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;

                    foreach (var (shopId, shopData) in shopsData)
                    {
                        OverhaulSeedAmounts(shopId, shopData);
                        FilterUnlockedSeeds(shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void OverhaulSeedAmounts(string shopId, ShopData shopData)
        {
            if (!ModEntry.Instance.Config.EnableSeedShopOverhaul)
            {
                return;
            }

            OverhaulSmallBusinessSeeds(shopId, shopData);
            OverhaulBigCorporationSeeds(shopId, shopData);
        }

        private void OverhaulSmallBusinessSeeds(string shopId, ShopData shopData)
        {
            var isPierre = shopId == "SeedShop";
            if (!isPierre && shopId != "Sandy")
            {
                return;
            }

            var hasStocklist = Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist");

            for (var i = shopData.Items.Count; i >= 0; i--)
            {
                var item = shopData.Items[i];
                var priceMultiplier = 1.0f;
                if (item.AvailableStock == -1)
                {
                    var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + item.GetHashCode());
                    var maxAmount = GetVillagerMaxAmountAndPrice(item.Condition, hasStocklist && isPierre, ref priceMultiplier);
                    var todayStock = random.Next(maxAmount);
                    if (todayStock < 5)
                    {
                        shopData.Items.RemoveAt(i);
                        continue;
                    }

                    item.AvailableStock = todayStock;
                    item.AvailableStockLimit = LimitedStockMode.Global;
                }

                if (isPierre)
                {
                    item.Price = (int)Math.Round(item.Price * priceMultiplier);
                }

                if (item.Condition != null && item.Condition.Contains("YEAR 2"))
                {
                    item.Condition = string.Join(", ", item.Condition.Split(",").Select(x => x.Trim()).Where(x => !x.Contains("YEAR 2")));
                }

                item.AvoidRepeat = true;
            }
        }

        private void OverhaulBigCorporationSeeds(string shopId, ShopData shopData)
        {
            if (shopId != "Joja")
            {
                return;
            }

            if (Game1.player.friendshipData.ContainsKey("Sandy"))
            {
                AddToJojaShop(shopData, QualifiedItemIds.RHUBARB_SEEDS);
                AddToJojaShop(shopData, QualifiedItemIds.STARFRUIT_SEEDS);
                AddToJojaShop(shopData, QualifiedItemIds.BEET_SEEDS);
            }

            if (TravelingMerchantInjections.HasAnyTravelingMerchantDay())
            {
                AddToJojaShop(shopData, QualifiedItemIds.RARE_SEED, 10, 1000);
            }
            
            AddToJojaShop(shopData, QualifiedItemIds.SUGAR, 20);
            AddToJojaShop(shopData, QualifiedItemIds.WHEAT_FLOUR, 20);
            AddToJojaShop(shopData, QualifiedItemIds.RICE, 20);
            AddToJojaShop(shopData, QualifiedItemIds.OIL, 20);
            AddToJojaShop(shopData, QualifiedItemIds.VINEGAR, 20);

            var objectsData = DataLoader.Objects(Game1.content);
            foreach (var item in shopData.Items)
            {
                if (!QualifiedItemIds.IsObject(item.ItemId))
                {
                    continue;
                }

                var itemData = objectsData[QualifiedItemIds.UnqualifyId(item.ItemId)];
                item.AvailableStock = -1;
                item.AvoidRepeat = true;
                if (item.MinStack == -1)
                {
                    item.MinStack = itemData.Name.Contains("cola", StringComparison.InvariantCultureIgnoreCase) ? 6 : 50;
                    item.Price *= item.MinStack;
                }
                
                item.Price = (int)Math.Round(item.Price * JOJA_PRICE_MULTIPLIER);
                item.Condition = null;
            }
        }

        private void AddToJojaShop(ShopData shopData, string itemId, int stack = -1, int price = -1)
        {
            var existingItem = shopData.Items.Find(x => x.ItemId.Equals(itemId, StringComparison.InvariantCultureIgnoreCase));
            if (existingItem != null)
            {
                shopData.Items.Remove(existingItem);
            }
            var item = new ShopItemData()
            {
                Id = itemId,
                ItemId = itemId,
                MinStack = stack,
                MaxStack = -1,
                Price = price,
                Condition = null,
            };
            shopData.Items.Add(item);
        }

        private void FilterUnlockedSeeds(ShopData shopData)
        {
            if (_archipelago.SlotData.Cropsanity != Cropsanity.Shuffled)
            {
                return;
            }

            var itemsData = DataLoader.Objects(Game1.content);
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!QualifiedItemIds.IsObject(item.ItemId))
                {
                    continue;
                }

                var itemData = itemsData[QualifiedItemIds.UnqualifyId(item.ItemId)];

                if (itemData.Category != Category.SEEDS || itemData.Name == "Mixed Seeds")
                {
                    continue;
                }

                item.Condition ??= "";
                var conditions = item.Condition.Split(",").Select(x => x.Trim()).ToList();
                conditions.Add(GameStateConditionProvider.CreateHasReceivedItemCondition(itemData.Name));
                item.Condition = string.Join(", ", conditions);
            }
        }

        private int GetVillagerMaxAmountAndPrice(string itemSeason, bool hasStocklist, ref float priceMultiplier)
        {
            var maxAmount = 20;
            if (itemSeason != null && itemSeason != Game1.currentSeason)
            {
                priceMultiplier *= 1.5f;
            }

            if (hasStocklist)
            {
                maxAmount *= 2;
            }

            var numberMovieTheater = _archipelago.GetReceivedItemCount(TheaterInjections.MOVIE_THEATER_ITEM);
            maxAmount *= (int)Math.Pow(2, numberMovieTheater);
            priceMultiplier *= (int)Math.Pow(1.5f, numberMovieTheater);

            return maxAmount;
        }
    }
}
