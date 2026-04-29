using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using StardewValley;

namespace StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization
{
    public class RandomizedData
    {
        public Dictionary<string, RandomizedCropData> Crops { get; set; }
        public Dictionary<string, RandomizedFishData> Fish { get; set; }
        public Dictionary<string, RandomizedFestivalData> Festivals { get; set; }
        public Dictionary<string, Dictionary<string, RandomizedShopItemData>> Shops { get; set; }

        public RandomizedData()
        {
            Crops = new Dictionary<string, RandomizedCropData>();
            Fish = new Dictionary<string, RandomizedFishData>();
            Festivals = new Dictionary<string, RandomizedFestivalData>();
            Shops = new Dictionary<string, Dictionary<string, RandomizedShopItemData>>();
        }

        public void AssignNames()
        {
            AssignCropNames();
            AssignFishNames();
            AssignFestivalNames();
            AssignShopNames();

            ValidateData();
        }

        private void AssignCropNames()
        {
            foreach (var (name, data) in Crops)
            {
                data.AssignName(name);
            }
        }

        private void AssignFishNames()
        {
            foreach (var (name, data) in Fish)
            {
                data.AssignName(name);
            }
        }

        private void AssignFestivalNames()
        {
            foreach (var (name, data) in Festivals)
            {
                data.AssignName(name);
            }
        }

        private void AssignShopNames()
        {
            foreach (var (shopName, shopData) in Shops)
            {
                foreach (var (itemName, shopItemData) in shopData)
                {
                    shopItemData.AssignNamesAndDefaults(shopName, itemName);
                }
            }
        }

        private void ValidateData()
        {
            var allShopNames = GetAllShopNames();
            var allItemEntries = GetAllItemEntries();
            var allCurrencies = GetAllCurrencies();
            var allMaterialEntries = GetAllMaterialEntries();

            ExportToJson("DR - ShopNames.json", allShopNames);
            ExportToJson("DR - Items.json", allItemEntries);
            ExportToJson("DR - Currencies.json", allCurrencies);
            ExportToJson("DR - Materials.json", allMaterialEntries);
        }

        private List<string> GetAllShopNames()
        {
            return Shops.Keys.ToHashSet().ToList();
        }

        private Dictionary<string, int> GetAllItemEntries()
        {
            var itemEntries = new Dictionary<string, int>();
            foreach (var (shopName, shopData) in Shops)
            {
                foreach (var (itemName, shopItemData) in shopData)
                {
                    if (!itemEntries.ContainsKey(itemName))
                    {
                        itemEntries.Add(itemName, 0);
                    }
                    itemEntries[itemName]++;
                }
            }

            return itemEntries;
        }

        private List<string> GetAllCurrencies()
        {
            var currencies = new HashSet<string>();
            foreach (var (shopName, shopData) in Shops)
            {
                foreach (var (itemName, shopItemData) in shopData)
                {
                    currencies.Add(shopItemData.Currency);
                }
            }

            return currencies.ToList();
        }

        private Dictionary<string, List<int>> GetAllMaterialEntries()
        {
            var materialEntries = new Dictionary<string, List<int>>();
            foreach (var (shopName, shopData) in Shops)
            {
                foreach (var (itemName, shopItemData) in shopData)
                {
                    if (shopItemData?.Materials == null)
                    {
                        continue;
                    }
                    foreach (var (materialItem, materialAmount) in shopItemData.Materials)
                    {
                        if (!materialEntries.ContainsKey(materialItem))
                        {
                            materialEntries.Add(materialItem, new List<int>());
                        }
                        materialEntries[materialItem].Add(materialAmount);
                    }
                }
            }

            return materialEntries;
        }

        private void ExportToJson(string jsonPath, object data)
        {
            var jsonText = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(jsonPath, jsonText);
        }
    }
}
