using System.Collections.Generic;
using Force.DeepCloner;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Shops;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class TravelingMerchantShopStockModifier : ShopStockModifier
    {
        public TravelingMerchantShopStockModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(logger, modHelper, archipelago, stardewItemManager)
        {
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
                    var cartShopData = shopsData["Traveler"];
                    SetUpRandomItemsStockSize(cartShopData);
                    AddMetalDetectorItems(cartShopData);
                    AddChecks(cartShopData);
                    RemoveWeddingRingCondition(cartShopData);
                    ReplaceEndgameLocationItems(cartShopData);
                },
                AssetEditPriority.Late - 1
            );
        }

        private void SetUpRandomItemsStockSize(ShopData cartShopData)
        {
            for (var i = 0; i < cartShopData.Items.Count; i++)
            {
                var item = cartShopData.Items[i];
                if (item.Id != "RandomObjects" || item.ItemId != "RANDOM_ITEMS (O) 2 789 @requirePrice @isRandomSale" || item.MaxItems <= 1 || !string.IsNullOrWhiteSpace(item.Condition))
                {
                    continue;
                }

                cartShopData.Items.RemoveAt(i);

                for (var numberItems = 0; numberItems < item.MaxItems; numberItems++)
                {
                    var newItem = item.DeepClone();
                    newItem.Id = $"RandomObjects[{numberItems}]";
                    newItem.MaxItems = 1;
                    newItem.Condition = GameStateConditionProvider.CreateHasStockSizeCondition(numberItems);
                    newItem.ActionsOnPurchase = new List<string> { TriggerActionProvider.TRAVELING_MERCHANT_PURCHASE };
                    cartShopData.Items.Insert(i + numberItems, newItem);
                }
            }
        }

        private void AddMetalDetectorItems(ShopData cartShopData)
        {
            var metalDetector = new ShopItemData()
            {
                Id = IDProvider.METAL_DETECTOR_ITEMS,
                ItemId = IDProvider.METAL_DETECTOR_ITEMS,
                AvailableStock = 1,
                IsRecipe = false,
                MaxItems = 10,
                AvoidRepeat = true,
            };
            cartShopData.Items.Add(metalDetector);
        }

        private void AddChecks(ShopData cartShopData)
        {
            var dailyCheck = new ShopItemData()
            {
                Id = IDProvider.TRAVELING_CART_DAILY_CHECK,
                ItemId = IDProvider.TRAVELING_CART_DAILY_CHECK,
                AvailableStock = 1,
                IsRecipe = false,
                MaxItems = 1,
            };
            cartShopData.Items.Add(dailyCheck);
        }

        private void RemoveWeddingRingCondition(ShopData cartShopData)
        {
            for (var i = 0; i < cartShopData.Items.Count; i++)
            {
                var item = cartShopData.Items[i];
                if (item.Condition == null)
                {
                    continue;
                }

                item.Condition = GameStateConditionProvider.RemoveCondition(item.Condition, "IS_MULTIPLAYER");
            }
        }

        private void ReplaceEndgameLocationItems(ShopData cartShopData)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return;
            }

            var createdSpousePortrait = false;
            for (var i = 0; i < cartShopData.Items.Count; i++)
            {
                createdSpousePortrait = ReplacePortraits(cartShopData, ref i, createdSpousePortrait);
                ReplaceTeaSet(cartShopData, ref i);
            }
        }

        private bool ReplacePortraits(ShopData cartShopData, ref int i, bool createdSpousePortrait)
        {
            var item = cartShopData.Items[i];
            var portrait = "Portrait";
            var unqualifiedItemId = QualifiedItemIds.UnqualifyId(item.ItemId, out var qualifier);
            if (qualifier != QualifiedItemIds.FURNITURE_QUALIFIER || !unqualifiedItemId.EndsWith(portrait))
            {
                return createdSpousePortrait;
            }

            var npcName = unqualifiedItemId[..unqualifiedItemId.IndexOf(portrait)];
            var locationName = $"{Prefix.PURCHASE}{npcName} {portrait}";

            if (_archipelago.SlotData.Friendsanity == Friendsanity.AllWithMarriage)
            {
                item.Condition = item.Condition.AddCondition(GameStateConditionProvider.CreateHasReceivedItemCondition($"{npcName} {portrait}"));
            }
            // shopData.Items.RemoveAt(i);

            var apShopItem = CreateArchipelagoLocation(item, locationName);
            cartShopData.Items.Insert(i, apShopItem);
            i++;

            if (!createdSpousePortrait)
            {
                var spouseLocationName = "Purchase Spouse Portrait";
                var apSpouseShopItem = CreateArchipelagoLocation(item, spouseLocationName);
                apSpouseShopItem.Condition = "PLAYER_HEARTS Current AnyDateable 14";
                cartShopData.Items.Insert(i, apSpouseShopItem);
                i++;
                createdSpousePortrait = true;
            }
            return createdSpousePortrait;
        }

        private void ReplaceTeaSet(ShopData cartShopData, ref int i)
        {
            var item = cartShopData.Items[i];
            if (item.ItemId != QualifiedItemIds.TEA_SET)
            {
                return;
            }
            
            var locationName = $"{Prefix.PURCHASE}Tea Set";
            
            // shopData.Items.RemoveAt(i);

            var apShopItem = CreateArchipelagoLocation(item, locationName);
            cartShopData.Items.Insert(i, apShopItem);
            i++;
        }
    }
}
