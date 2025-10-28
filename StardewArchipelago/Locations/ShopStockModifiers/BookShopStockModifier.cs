using Force.DeepCloner;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.Shops;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using KaitoKid.Utilities.Interfaces;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class BookShopStockModifier : ShopStockModifier
    {
        public BookShopStockModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, helper, archipelago, stardewItemManager)
        {
            _logger = logger;
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
                var booksellerShop = shopsData["Bookseller"];

                var booksellerShopAsJson = JsonConvert.SerializeObject(booksellerShop);
                File.WriteAllText("booksellerShop_original.json", booksellerShopAsJson);

                ChangeSalesBasedOnApUnlocks(booksellerShop);
                ApplyPriceMultiplier(booksellerShop);

                booksellerShopAsJson = JsonConvert.SerializeObject(booksellerShop);
                File.WriteAllText("booksellerShop_modified.json", booksellerShopAsJson);
            },
                AssetEditPriority.Late
            );
        }

        private void ChangeSalesBasedOnApUnlocks(ShopData shopData)
        {
            var permanentBookShopIds = new[]
            {
                QualifiedItemIds.PRICE_CATALOGUE, QualifiedItemIds.QUEEN_OF_SAUCE_COOKBOOK, QualifiedItemIds.OL_SLITHERLEGS, QualifiedItemIds.HORSE_THE_BOOK,
                QualifiedItemIds.WAY_OF_THE_WIND_1, QualifiedItemIds.WAY_OF_THE_WIND_2, QualifiedItemIds.BOOK_OF_STARS,
            };

            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];

                if (permanentBookShopIds.Contains(item.Id))
                {
                    AddConditionForPermanentBook(shopData, i);
                    continue;
                }
                if (item.Id == QualifiedItemIds.WOODCUTTER_WEEKLY && item.Condition.StartsWith("SYNCED_RANDOM"))
                {
                    RemoveFromShop(shopData, i);
                    continue;
                }
                if (item.Id == "RandomBook")
                {
                    ReplaceWithRareBooks(shopData, i);
                    continue;
                }
                if (item.Id.StartsWith("RandomSkillBook"))
                {
                    ReplaceWithExperienceBooks(shopData, i);
                    continue;
                }
            }
        }

        private void AddConditionForPermanentBook(ShopData shopData, int itemIndex)
        {
            var item = shopData.Items[itemIndex];
            if (item.Condition == null)
            {
                item.Condition = "";
            }
            if (item.Condition.StartsWith("SYNCED_RANDOM", StringComparison.InvariantCultureIgnoreCase))
            {
                item.Condition = "";
            }

            item.Condition = item.Condition.AddCondition(GameStateConditionProvider.CreateHasReceivedItemCondition(APItem.BOOKSELLER_PERMANENT_BOOKS));
        }

        private void RemoveFromShop(ShopData shopData, int itemIndex)
        {
            shopData.Items.RemoveAt(itemIndex);
        }

        private void ReplaceWithRareBooks(ShopData shopData, int itemIndex)
        {
            var item = shopData.Items[itemIndex].DeepClone();
            shopData.Items.RemoveAt(itemIndex);
            var rareBooks = new[]
            {
                QualifiedItemIds.THE_ALLEYWAY_BUFFET, QualifiedItemIds.THE_ART_O_CRABBING, QualifiedItemIds.DWARVISH_SAFETY_MANUAL,
                QualifiedItemIds.JEWELS_OF_THE_SEA, QualifiedItemIds.WOODYS_SECRET, QualifiedItemIds.MAPPING_CAVE_SYSTEMS,
                QualifiedItemIds.JACK_BE_NIMBLE_JACK_BE_THICK, QualifiedItemIds.FRIENDSHIP_101, QualifiedItemIds.MONSTER_COMPENDIUM,
                // QualifiedItemIds.RACCOON_JOURNAL, // Removed from bookseller due to being reliable in-logic at the raccoons
            };

            var received1Condition = GameStateConditionProvider.CreateHasReceivedItemExactAmountCondition(APItem.BOOKSELLER_RARE_BOOKS, 1);
            var received2Condition = GameStateConditionProvider.CreateHasReceivedItemCondition(APItem.BOOKSELLER_RARE_BOOKS, 2);

            var booksToInsert = new List<ShopItemData>();
            foreach (var rareBook in rareBooks)
            {
                var rareBookToInsert = item.DeepClone();
                var random1Condition = $"SYNCED_RANDOM day {rareBook}sale1 0.5";
                var firstCondition = GameStateConditionProvider.ConcatenateConditions(new[] { received1Condition, random1Condition }, false);
                var fullCondition = GameStateConditionProvider.CreateOrCondition(new []{firstCondition, received2Condition});
                rareBookToInsert.Condition = fullCondition;
                rareBookToInsert.Id = rareBook;
                rareBookToInsert.ItemId = rareBook;
                rareBookToInsert.RandomItemId = null;
                booksToInsert.Add(rareBookToInsert);
            }

            shopData.Items.InsertRange(itemIndex, booksToInsert);
        }

        private void ReplaceWithExperienceBooks(ShopData shopData, int itemIndex)
        {
            var item = shopData.Items[itemIndex].DeepClone();
            shopData.Items.RemoveAt(itemIndex);

            var experienceBooks = new[]
            {
                QualifiedItemIds.STARDEW_VALLEY_ALMANAC, QualifiedItemIds.BAIT_AND_BOBBER, QualifiedItemIds.MINING_MONTHLY,
                QualifiedItemIds.COMBAT_QUARTERLY, QualifiedItemIds.WOODCUTTER_WEEKLY,
            };

            // "RandomSkillBook3"
            var groupNumber = int.Parse(item.Id.Last().ToString());
            var price = groupNumber switch
            {
                1 => 10000,
                2 => 8000,
                3 => 5000,
                _ => 2000,
            };
            var receivedCondition = GameStateConditionProvider.CreateHasReceivedItemExactAmountCondition(APItem.BOOKSELLER_EXPERIENCE_BOOKS, groupNumber);
            if (groupNumber == 3)
            {
                receivedCondition = GameStateConditionProvider.CreateHasReceivedItemCondition(APItem.BOOKSELLER_EXPERIENCE_BOOKS, 3);
            }

            var booksToInsert = new List<ShopItemData>();
            foreach (var experienceBook in experienceBooks)
            {
                var experienceBookToInsert = item.DeepClone();
                experienceBookToInsert.Condition = receivedCondition;
                experienceBookToInsert.Condition = receivedCondition;
                experienceBookToInsert.Id = experienceBook;
                experienceBookToInsert.ItemId = experienceBook;
                experienceBookToInsert.Price = price;
                experienceBookToInsert.RandomItemId = null;
                booksToInsert.Add(experienceBookToInsert);
            }

            shopData.Items.InsertRange(itemIndex, booksToInsert);
        }

        private void ApplyPriceMultiplier(ShopData shopData)
        {
            if (ModEntry.Instance.Config.BooksellerPriceMultiplier == 100)
            {
                return;
            }

            var multiplier = ModEntry.Instance.Config.BooksellerPriceMultiplier / 100f;
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];

                if (item.PriceModifiers == null)
                {
                    item.PriceModifiers = new List<QuantityModifier>();
                }

                var modifier = new QuantityModifier
                {
                    Amount = multiplier,
                    Id = nameof(ModEntry.Instance.Config.BooksellerPriceMultiplier),
                    Modification = QuantityModifier.ModificationType.Multiply,
                };
                item.PriceModifiers.Add(modifier);
            }
        }
    }
}
