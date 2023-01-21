using System;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;

namespace StardewArchipelago.Items
{
    public class ItemParser
    {
        public const string RESOURCE_PACK_PREFIX = "Resource Pack: ";
        public const string FRIENDSHIP_BONUS_PREFIX = "Friendship Bonus (";
        public const string BUILDING_PREFIX = "Building: ";

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
                return resourcePackItem.GetAsLetter(receivedItem, resourcePackAmount);
            }

            var itemIsFriendshipBonus = TryParseFriendshipBonus(receivedItem.ItemName, out var numberOfPoints);
            if (itemIsFriendshipBonus)
            {
                return new LetterActionAttachment(receivedItem, LetterActionsKeys.Frienship, numberOfPoints.ToString());
            }

            if (_unlockManager.IsUnlock(receivedItem.ItemName))
            {
                return _unlockManager.PerformUnlock(receivedItem);
            }

            if (_itemManager.ItemExists(receivedItem.ItemName))
            {
                var singleItem = GetSingleItem(receivedItem.ItemName);
                return singleItem.GetAsLetter(receivedItem);
            }

            return new LetterAttachment(receivedItem);
            throw new ArgumentException($"Could not process item {receivedItem.ItemName}");
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

        private bool TryParseBuilding(string apItemName, out string buildingName)
        {
            buildingName = "";
            if (!apItemName.StartsWith(BUILDING_PREFIX))
            {
                return false;
            }

            buildingName = apItemName.Substring(BUILDING_PREFIX.Length);
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
    }
}
