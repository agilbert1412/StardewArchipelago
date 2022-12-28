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

        public void TestGetAllItems(string arg1, string[] arg2)
        {
            _itemParser = new ItemParser(new StardewItemManager(), new UnlockManager());

            var itemsTable = _helper.Data.ReadJsonFile<Dictionary<string, JObject>>("stardew_valley_item_table.json");
            var items = itemsTable["items"];
            foreach (var (key, jEntry) in items)
            {
                var code = jEntry["code"].Value<long>();
                var classification = Enum.Parse<ItemClassification>(jEntry["classification"].Value<string>(), true);
                var item = new ArchipelagoItem(key, code, classification);
                try
                {
                    var actionToProcessItem = _itemParser.GetActionToProcessItem(item.Name, 1, 1);
                    actionToProcessItem();
                    Debug.Assert(actionToProcessItem != null);
                }
                catch (Exception e)
                {
                    _monitor.Log($"Item: \"{key}\" was not processed properly by the mod", LogLevel.Error);
                }
            }
        }

        public void TestSendAllLocations(string arg1, string[] arg2)
        {
            throw new NotImplementedException();
        }
    }
}
