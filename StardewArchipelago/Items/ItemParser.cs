using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class ItemParser
    {
        private const string RESOURCE_PACK_PREFIX = "Resource Pack: ";
        private const string FRIENDSHIP_BONUS_PREFIX = "Friendship Bonus (";


        private StardewItemManager _itemManager;
        private UnlockManager _unlockManager;

        public ItemParser(StardewItemManager itemManager, UnlockManager unlockManager)
        {
            _itemManager = itemManager;
            _unlockManager = unlockManager;
        }

        public LetterAttachment ProcessItem(ReceivedItem receivedItem)
        {
            var itemIsResourcePack = TryParseResourcePack(receivedItem.ItemName, out var stardewItemName, out var resourcePackAmount);
            if (itemIsResourcePack)
            {
                if (stardewItemName == "Money")
                {
                    return new LetterMoneyAttachment(receivedItem, resourcePackAmount);
                }

                var resourcePackItem = GetResourcePackItem(stardewItemName);
                return new LetterItemAttachment(receivedItem, resourcePackItem, resourcePackAmount);
            }

            var itemIsFriendshipBonus = TryParseFriendshipBonus(receivedItem.ItemName, out var numberOfPoints);
            if (itemIsFriendshipBonus)
            {
                return new LetterCustomAttachment(receivedItem, openedAction: () => IncreaseFriendshipWithEveryone(numberOfPoints));
            }

            if (_unlockManager.IsUnlock(receivedItem.ItemName))
            {
                return new LetterCustomAttachment(receivedItem, openedAction: () => _unlockManager.PerformUnlock(receivedItem.ItemName, numberReceived));
            }

            if (_itemManager.ItemExists(receivedItem.ItemName))
            {
                var resourcePackItem = GetSingleItem(receivedItem.ItemName);
                return new LetterItemAttachment(receivedItem, resourcePackItem);
            }

            throw new ArgumentException($"Could not process item {receivedItem.ItemName}");
        }

        public void ProcessUnlockWithoutGivingNewItems(string itemName, int numberReceived)
        {
            if (!_unlockManager.IsUnlock(itemName))
            {
                return;
            }

            _unlockManager.ShouldGiveItemsWithUnlocks = false;
            _unlockManager.PerformUnlock(itemName, numberReceived);
            _unlockManager.ShouldGiveItemsWithUnlocks = true;
        }

        private bool TryParseResourcePack(string apItemName, out string stardewItemName, out int amount)
        {
            stardewItemName = "";
            amount = 0;
            if (!apItemName.StartsWith(RESOURCE_PACK_PREFIX))
            {
                return false;
            }

            var apItemWithoutPrefix = apItemName.Substring(RESOURCE_PACK_PREFIX.Length);
            var parts = apItemWithoutPrefix.Split(" ");
            if (!int.TryParse(parts[0], out amount))
            {
                return false;
            }

            stardewItemName = apItemWithoutPrefix.Substring(apItemWithoutPrefix.IndexOf(" ", StringComparison.Ordinal) + 1);
            return true;
        }

        private bool TryParseFriendshipBonus(string apItemName, out int numberOfPoints)
        {
            numberOfPoints = 0;
            if (!apItemName.StartsWith(FRIENDSHIP_BONUS_PREFIX))
            {
                return false;
            }

            var apItemWithoutPrefix = apItemName.Substring(FRIENDSHIP_BONUS_PREFIX.Length);
            var parts = apItemWithoutPrefix.Split(" ");
            if (!double.TryParse(parts[0], out var numberOfHearts))
            {
                return false;
            }

            numberOfPoints = (int)Math.Round(numberOfHearts * 250);

            return true;
        }

        private StardewItem GetSingleItem(string stardewItemName)
        {
            var item = _itemManager.GetItemByName(stardewItemName);
            return item;
        }

        private StardewItem GetResourcePackItem(string stardewItemName)
        {
            var item = _itemManager.GetItemByName(stardewItemName);
            return item;
        }

        private void IncreaseFriendshipWithEveryone(int numberOfPoints)
        {
            var farmer = Game1.player;
            foreach (var npc in farmer.friendshipData.Keys)
            {
                farmer.friendshipData[npc].Points += numberOfPoints;
            }
        }
    }
}
