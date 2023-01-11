using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Test
{
    public class Tester
    {
        private IModHelper _helper;
        private IMonitor _monitor;
        private ItemParser _itemParser;
        private Mailman _mail;

        public Tester(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
            _mail = new Mailman();
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
            var receivedItem = new ReceivedItem("locationName", itemName, "playerName", 1, 2, 3);

            _itemParser = new ItemParser(new StardewItemManager(), new UnlockManager(_mail));
            try
            {
                var attachment = _itemParser.ProcessItem(receivedItem);
                attachment.SendToPlayer(_mail);
            }
            catch (Exception e)
            {
                _monitor.Log($"Item: \"{itemName}\" was not processed properly by the mod", LogLevel.Error);
            }
        }

        public void TestGetAllItems(string arg1, string[] arg2)
        {
            _itemParser = new ItemParser(new StardewItemManager(), new UnlockManager(_mail));

            var itemsTable = _helper.Data.ReadJsonFile<Dictionary<string, JObject>>("stardew_valley_item_table.json");
            var items = itemsTable["items"];
            var attachments = new List<LetterItemAttachment>();
            foreach (var (key, jEntry) in items)
            {
                var code = jEntry["code"].Value<long>();
                var classification = Enum.Parse<ItemClassification>(jEntry["classification"].Value<string>(), true);
                var item = new ArchipelagoItem(key, code, classification);
                var receivedItem = new ReceivedItem("locationName", key, "playerName", 1, code, 3);
                try
                {
                    var attachment = _itemParser.ProcessItem(receivedItem);
                    attachment.SendToPlayer(_mail);
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
