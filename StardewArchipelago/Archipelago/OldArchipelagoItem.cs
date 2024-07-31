//using System;
//using System.Collections.Generic;
//using System.IO;
//using Newtonsoft.Json.Linq;
//using StardewModdingAPI;

//namespace StardewArchipelago.Archipelago
//{
//    public class ArchipelagoItem
//    {
//        public string Name { get; set; }
//        public long Id { get; set; }
//        public ItemClassification Classification { get; set; }

//        public ArchipelagoItem(string name, long id, ItemClassification classification)
//        {
//            Name = name;
//            Id = id;
//            Classification = classification;
//        }

//        public static IEnumerable<ArchipelagoItem> LoadItems(IModHelper helper)
//        {
//            var pathToItemTable = Path.Combine("IdTables", "stardew_valley_item_table.json");
//            var itemsTable = helper.Data.ReadJsonFile<Dictionary<string, JObject>>(pathToItemTable);
//            var items = itemsTable["items"];
//            foreach (var (key, jEntry) in items)
//            {
//                yield return LoadItem(key, jEntry);
//            }
//        }

//        private static ArchipelagoItem LoadItem(string itemName, JToken itemJson)
//        {
//            var id = itemJson["code"].Value<long>();
//            var classification = Enum.Parse<ItemClassification>(itemJson["classification"].Value<string>(), true);
//            var item = new ArchipelagoItem(itemName, id, classification);
//            return item;
//        }
//    }

//    [Flags]
//    public enum ItemClassification
//    {
//        Filler = 0b0000,
//        Progression = 0b0001,
//        Useful = 0b0010,
//        Trap = 0b0100,
//        SkipBalancing = 0b1000,
//        ProgressionSkipBalancing = Progression | SkipBalancing,
//        skip_balancing = SkipBalancing,
//        progression_skip_balancing = ProgressionSkipBalancing,
//    }
//}

