using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.Festival;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;
using Category = StardewArchipelago.Constants.Vanilla.Category;

namespace StardewArchipelago.Locations.ShopStockModifiers
{
    public class FestivalShopStockModifier : ShopStockModifier
    {
        public FestivalShopStockModifier(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, StardewItemManager stardewItemManager) : base(monitor, modHelper, archipelago, stardewItemManager)
        {
        }

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (_archipelago.SlotData.FestivalLocations == FestivalLocations.Vanilla)
            {
                return;
            }

            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;

                    foreach (var (shopId, shopData) in shopsData)
                    {
                        AddArchipelagoChecks(shopId, shopData);
                    }
                },
                AssetEditPriority.Late
            );
        }

        private void AddArchipelagoChecks(string shopId, ShopData shopData)
        {
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                if (!IsItemKnownAsACheck(item, out var locationName, out var itemName))
                {
                    continue;
                }

                var existingConditions = item.Condition.Split(",", StringSplitOptions.RemoveEmptyEntries);
                var apShopitem = CreateArchipelagoLocation(item, locationName);
                shopData.Items.Add(apShopitem);
                AddArchipelagoCondition(item, existingConditions, itemName);
            }
        }

        private bool IsItemKnownAsACheck(ShopItemData item, out string locationName, out string itemName)
        {
            if (_archipelago.LocationExists(item.ObjectInternalName) || _archipelago.LocationExists(item.ItemId))
            {
                locationName = item.ObjectInternalName;
                itemName = item.ObjectInternalName;
                return true;
            }

            locationName = null;
            itemName = null;
            if (item.ItemId == null)
            {
                return false;
            }

            var bigCraftablesData = DataLoader.BigCraftables(Game1.content);
            if (bigCraftablesData.ContainsKey(item.ItemId))
            {
                var bigCraftableData = bigCraftablesData[item.ItemId];
                if (_archipelago.LocationExists(bigCraftableData.Name))
                {
                    locationName = bigCraftableData.Name;
                    itemName = bigCraftableData.Name;
                    return true;
                }

                if (IsRarecrow(item.ItemId, bigCraftableData, out var rarecrowNumber))
                {
                    GetRarecrowCheckName(rarecrowNumber, out locationName, out itemName);
                    return true;
                }

                return false;
            }

            var objectsData = DataLoader.Objects(Game1.content);
            if (objectsData.ContainsKey(item.ItemId))
            {
                var objectData = objectsData[item.ItemId];
                if (_archipelago.LocationExists(objectData.Name))
                {
                    locationName = objectData.Name;
                    itemName = objectData.Name;
                    return true;
                }

                if (objectData.Category == Category.SEEDS && objectData.Name == "Strawberry Seeds")
                {
                    locationName = FestivalLocationNames.STRAWBERRY_SEEDS;
                    itemName = "Strawberry Seeds";
                    return true;
                }

                if (objectData.Name == "Stardrop" && item.Condition.Contains("PLAYER_HAS_MAIL Current CF_Fair"))
                {
                    locationName = FestivalLocationNames.FAIR_STARDROP;
                    itemName = objectData.Name;
                    return true;
                }

                return false;
            }


            var hatsData = DataLoader.Hats(Game1.content);
            if (hatsData.ContainsKey(item.ItemId))
            {
                var hatData = hatsData[item.ItemId];
                var hatName = hatData.Split('/')[0];
                if (_archipelago.LocationExists(hatName))
                {
                    locationName = hatName;
                    itemName = hatName;
                    return true;
                }

                return false;
            }

            var furnituresData = DataLoader.Furniture(Game1.content);
            if (furnituresData.ContainsKey(item.ItemId))
            {
                var furnitureData = furnituresData[item.ItemId];
                var furnitureName = furnitureData.Split('/')[0];
                if (_archipelago.LocationExists(furnitureName))
                {
                    locationName = furnitureName;
                    itemName = furnitureName;
                    return true;
                }

                return false;
            }

            return false;
        }

        private bool IsRarecrow(string bigCraftableId, BigCraftableData bigCraftableData, out int rarecrowNumber)
        {
            if (!bigCraftableData.Name.Equals("Rarecrow", StringComparison.InvariantCultureIgnoreCase))
            {
                rarecrowNumber = 0;
                return false;
            }


            rarecrowNumber = bigCraftableId switch
            {
                "110" => 1,
                "113" => 2,
                "126" => 3,
                "136" => 4,
                "137" => 5,
                "138" => 6,
                "139" => 7,
                "140" => 8,
                _ => 0,
            };
            return true;
        }

        public void GetRarecrowCheckName(int rarecrowNumber, out string locationName, out string itemName)
        {
            locationName = rarecrowNumber switch
            {
                1 => FestivalLocationNames.RARECROW_1,
                2 => FestivalLocationNames.RARECROW_2,
                3 => FestivalLocationNames.RARECROW_3,
                4 => FestivalLocationNames.RARECROW_4,
                5 => FestivalLocationNames.RARECROW_5,
                6 => FestivalLocationNames.RARECROW_6,
                7 => FestivalLocationNames.RARECROW_7,
                8 => FestivalLocationNames.RARECROW_8,
            };

            itemName = locationName.Split("(")[0].Trim();
        }
    }
}
