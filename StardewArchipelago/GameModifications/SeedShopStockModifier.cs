using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.CodeInjections;
using StardewArchipelago.Locations;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
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
                        FilterUnlockedSeeds(shopId, shopData);
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
            if (shopId != "SeedShop" && shopId != "Sandy")
            {
                return;
            }

            var isPierre = shopId == "SeedShop";
            var hasStocklist = Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist");

            foreach (var item in shopData.Items)
            {
                var priceMultiplier = 1.0f;
                if (item.AvailableStock == -1)
                {
                    var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + item.ItemId.GetHashCode());
                    var maxAmount = GetVillagerMaxAmountAndPrice(item.Condition, hasStocklist && isPierre, ref priceMultiplier);
                    var todayStock = random.Next(maxAmount);
                    if (todayStock < 5)
                    {
                        todayStock = 0;
                    }

                    item.AvailableStock = todayStock;
                    item.AvailableStockLimit = LimitedStockMode.Global;
                }

                if (isPierre && priceMultiplier != 1.0f)
                {
                    item.PriceModifierMode = QuantityModifier.QuantityModifierMode.Stack;
                    item.PriceModifiers.Add(new QuantityModifier() { Amount = priceMultiplier, Id = "Archipelago.SeedShopOverhaul", Modification = QuantityModifier.ModificationType.Multiply });
                }

                if (item.Condition.Contains("YEAR 2"))
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

            var itemsData = DataLoader.Objects(Game1.content);
            foreach (var item in shopData.Items)
            {
                if (!QualifiedItemIds.IsObject(item.ItemId))
                {
                    continue;
                }

                var itemData = itemsData[QualifiedItemIds.UnqualifyId(item.ItemId)];
                item.AvailableStock = -1;
                item.AvoidRepeat = true;
                if (item.MinStack == -1)
                {
                    item.MinStack = itemData.Name.Contains("cola", StringComparison.InvariantCultureIgnoreCase) ? 6 : 50;
                }
                
                item.PriceModifierMode = QuantityModifier.QuantityModifierMode.Stack;
                if (item.PriceModifiers == null)
                {
                    item.PriceModifiers = new List<QuantityModifier>();
                }
                item.PriceModifiers.Add(new QuantityModifier() { Amount = JOJA_PRICE_MULTIPLIER, Id = "Archipelago.SeedShopOverhaul", Modification = QuantityModifier.ModificationType.Multiply });
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

        private void FilterUnlockedSeeds(string shopId, ShopData shopData)
        {
            if (_archipelago.SlotData.Cropsanity != Cropsanity.Shuffled)
            {
                return;
            }

            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!DataLoader.Objects(Game1.content).TryGetValue(item.ItemId, out var itemData))
                {
                    continue;
                }

                if (itemData.Category != Category.SEEDS || itemData.Name == "Mixed Seeds")
                {
                    continue;
                }

                if (!_archipelago.HasReceivedItem(itemData.Name))
                {
                    shopData.Items.RemoveAt(i);
                }
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
