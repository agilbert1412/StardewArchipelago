using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using StardewArchipelago.Items;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Test
{
    public class Tester
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ItemParser _itemParser;

        public Tester(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        public void TestGetSpecificItem(string arg1, string[] arg2)
        {
            if (arg2.Length < 2)
            {
                return;
            }
            var amount = 1;
            amount = int.Parse(arg2[0]);

            var itemName = string.Join(" ", arg2.Skip(1).ToArray());

            _itemParser = new ItemParser(new StardewItemManager(), new UnlockManager(), new SpecialItemManager());
            try
            {
                var itemToReceive = _itemParser.ProcessItem(itemName, amount, amount);

                if (itemToReceive == null)
                {
                    return;
                }

                Game1.player.addItemByMenuIfNecessary(itemToReceive);
            }
            catch (Exception e)
            {
                _monitor.Log($"Item: \"{itemName}\" was not processed properly by the mod", LogLevel.Error);
            }
        }

        public void TestGetAllItems(string arg1, string[] arg2)
        {
            _itemParser = new ItemParser(new StardewItemManager(), new UnlockManager(), new SpecialItemManager());

            var itemsTable = _helper.Data.ReadJsonFile<Dictionary<string, JObject>>("stardew_valley_item_table.json");
            var items = itemsTable["items"];
            var itemsToGiveFarmer = new List<Item>();
            foreach (var (key, jEntry) in items)
            {
                var code = jEntry["code"].Value<long>();
                var classification = Enum.Parse<ItemClassification>(jEntry["classification"].Value<string>(), true);
                var item = new ArchipelagoItem(key, code, classification);
                try
                {
                    var itemToReceive = _itemParser.ProcessItem(item.Name, 1, 1);
                    
                    if (itemToReceive == null)
                    {
                        continue;
                    }

                    itemsToGiveFarmer.Add(itemToReceive);


                }
                catch (Exception e)
                {
                    _monitor.Log($"Item: \"{key}\" was not processed properly by the mod", LogLevel.Error);
                }
            }

            var itemsToDrop = new List<Item>();
            const int maxItemsToGive = 36;
            if (itemsToGiveFarmer.Count() > maxItemsToGive)
            {
                itemsToDrop.AddRange(itemsToGiveFarmer.Skip(maxItemsToGive));
                itemsToGiveFarmer = itemsToGiveFarmer.Take(maxItemsToGive).ToList();
            }

            Game1.player.addItemsByMenuIfNecessary(itemsToGiveFarmer);

            foreach (var itemToDrop in itemsToDrop)
            {
                if (itemToDrop == null)
                {
                    continue;
                }

                Game1.createItemDebris(itemToDrop, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
            }
        }

        public void TestSendAllLocations(string arg1, string[] arg2)
        {
            throw new NotImplementedException();
        }
    }
}
