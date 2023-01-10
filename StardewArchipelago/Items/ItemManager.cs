using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Items
{
    internal class ItemManager
    {
        private ArchipelagoClient _archipelago;
        private ItemParser _itemParser;
        private Mailman _mail;
        private HashSet<ReceivedItem> _itemsAlreadyProcessed;

        public ItemManager(ArchipelagoClient archipelago, StardewItemManager itemManager, UnlockManager unlockManager, IEnumerable<ReceivedItem> itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemParser = new ItemParser(itemManager, unlockManager);
            _mail = new Mailman();
            _itemsAlreadyProcessed = itemsAlreadyProcessed.ToHashSet();
        }

        public void RegisterAllUnlocks()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItemNamesAndCounts();
            foreach (var (itemName, numberReceived) in allReceivedItems)
            {
                _itemParser.ProcessUnlockWithoutGivingNewItems(itemName, numberReceived);
            }
        }

        public void ReceiveAllNewItems()
        {
            var allReceivedItems = _archipelago.GetAllReceivedItems();

            foreach (var receivedItem in allReceivedItems)
            {
                ReceiveNewItem(receivedItem);
            }
        }

        private void ReceiveNewItem(ReceivedItem receivedItem)
        {
            if (_itemsAlreadyProcessed.Contains(receivedItem))
            {
                return;
            }

            var attachment = _itemParser.ProcessItem(receivedItem);
            _mail.SendArchipelagoMail(receivedItem.ItemName, receivedItem.PlayerName, receivedItem.LocationName, attachment);
            _itemsAlreadyProcessed.Add(receivedItem);
        }

        public List<ReceivedItem> GetAllItemsAlreadyProcessed()
        {
            return _itemsAlreadyProcessed.ToList();
        }
    }
}
