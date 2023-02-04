using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Archipelago
{
    public class GiftHandler
    {
        private const string gift_key_pattern = "{0} {1}";

        public ArchipelagoClient _archipelago;
        private StardewItemManager _itemManager;
        private Mailman _mail;
        private Random _random;

        public GiftHandler()
        {
        }

        public void Initialize(StardewItemManager itemManager, Mailman mail, ArchipelagoClient archipelago)
        {
            _itemManager = itemManager;
            _mail = mail;
            _archipelago = archipelago;
            _random = new Random();
        }

        public bool HandleGiftItemCommand(string message)
        {
            if (_archipelago == null)
            {
                return false;
            }

            var giftPrefix = $"{ChatForwarder.COMMAND_PREFIX}gift ";
            if (!message.StartsWith(giftPrefix))
            {
                return false;
            }

            var receiverSlotName = message.Substring(giftPrefix.Length);
            var isValidRecipient = _archipelago.IsStardewValleyPlayer(receiverSlotName);

            if (!isValidRecipient)
            {
                Game1.chatBox?.addMessage($"{receiverSlotName} is not recognized as a Stardew Valley player in this multiworld", Color.Gold);
                return true;
            }

#if RELEASE
            if (receiverSlotName == _archipelago.SlotData.SlotName)
            {
                Game1.chatBox?.addMessage($"You cannot send yourself a gift", Color.Gold);
                return true;
            }
#endif
            var giftObject = Game1.player.ActiveObject;
            if (giftObject == null || !_itemManager.ItemExists(giftObject.Name) || _itemManager.GetItemByName(giftObject.Name) is not StardewObject)
            {
                Game1.chatBox?.addMessage($"You cannot gift this item to another player", Color.Gold);
                return true;
            }

            var itemNumber = 0;
            while (_archipelago.ExistsInDataStorage(string.Format(gift_key_pattern, receiverSlotName, itemNumber)))
            {
                itemNumber++;
            }

            var key = string.Format(gift_key_pattern, receiverSlotName, itemNumber);
            var sender = _archipelago.SlotData.SlotName.Replace(" ", "");
            var amount = giftObject.Stack;
            var itemName = giftObject.Name;
            var itemType = giftObject.bigCraftable.Value ? "bigobject" : "object"; // Stardew object types
            var value = $"{_random.Next(int.MaxValue)} {sender} {receiverSlotName.Replace(" ", "")} {amount} {itemType} {itemName}";

            _archipelago.SetDataStorage(key, value);
            Game1.player.ActiveObject = null;

            Game1.chatBox?.addMessage($"{receiverSlotName} will receive your gift of {amount} {itemName} within 1 business day", Color.Gold);
            Game1.chatBox?.addMessage($"Thank you for using Joja Prime", Color.Gold);

            return true;
        }

        public void ReceiveAllGiftsTomorrow()
        {
            if (_archipelago == null)
            {
                return;
            }

            var mySlotName = _archipelago.SlotData.SlotName;
            var itemNumber = 0;
            var key = string.Format(gift_key_pattern, mySlotName, itemNumber);
            while (_archipelago.ExistsInDataStorage(key))
            {
                ReceiveGiftTomorrow(key);
                itemNumber++;
                key = string.Format(gift_key_pattern, mySlotName, itemNumber);
            }

            var myAlias = _archipelago.GetPlayerAlias(mySlotName);
            itemNumber = 0;
            key = string.Format(gift_key_pattern, myAlias, itemNumber);
            while (_archipelago.ExistsInDataStorage(key))
            {
                ReceiveGiftTomorrow(key);
                itemNumber++;
                key = string.Format(gift_key_pattern, mySlotName, itemNumber);
            }
        }

        private void ReceiveGiftTomorrow(string giftKey)
        {
            var giftValue = _archipelago.ReadFromDataStorage(giftKey);
            var splitGiftValue = giftValue.Split(" ");
            var uniqueId = splitGiftValue[0];
            var senderName = splitGiftValue[1];
            var receiverName = splitGiftValue[2];
            var amount = splitGiftValue[3];
            var itemType = splitGiftValue[4];
            var itemName = string.Join(" ", splitGiftValue.Skip(5));

            var parsedItem = _itemManager.GetItemByName(itemName);

            var letterEmbedString = $"%item {itemType} {parsedItem.Id} {amount} %%";
            var mailKey = giftKey + "|" + giftValue;
            _mail.SendArchipelagoGiftMail(mailKey, senderName, letterEmbedString);
            _archipelago.RemoveFromDataStorage(giftKey);
        }
    }
}
