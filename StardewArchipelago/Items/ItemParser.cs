﻿using System;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Items.Unlocks;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.NameMapping;
using StardewModdingAPI;
using StardewValley;

#if TILESANITY
using StardewArchipelago.GameModifications.CodeInjections.Tilesanity;
#endif

namespace StardewArchipelago.Items
{
    public class ItemParser
    {
        public const string RESOURCE_PACK_PREFIX = "Resource Pack: ";
        public const string FRIENDSHIP_BONUS_PREFIX = "Friendship Bonus (";
        public const string RECIPE_SUFFIX = " Recipe";

        private readonly ILogger _logger;
        private readonly StardewItemManager _itemManager;
        private readonly UnlockManager _unlockManager;
        private readonly TrapManager _trapManager;

        // When More mods start to need name mapping, we can make a generic version of this
        private readonly CompoundNameMapper _nameMapper;

        public ItemParser(ILogger logger, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager, TrapExecutor trapExecutor, GiftTrapManager giftTrapManager)
        {
            _logger = logger;
            _itemManager = itemManager;
            _unlockManager = new UnlockManager(helper, archipelago, locationChecker);
            _trapManager = new TrapManager(logger, helper, harmony, archipelago, trapExecutor, giftTrapManager);
            _nameMapper = new CompoundNameMapper(archipelago.SlotData);
        }

        public TrapManager TrapManager => _trapManager;

        public bool TrySendItemImmediately(ReceivedItem receivedItem)
        {
#if TILESANITY
            if (TileUI.ProcessItem(receivedItem))
                return true;
#endif

            if (_trapManager.IsTrap(receivedItem.ItemName))
            {
                return _trapManager.TryExecuteTrapImmediately(receivedItem.ItemName);
            }

            return false;
        }

        public LetterAttachment ProcessItemAsLetter(ReceivedItem receivedItem)
        {
            var itemIsResourcePack = TryParseResourcePack(receivedItem.ItemName, out var stardewItemName, out var resourcePackAmount);
            if (itemIsResourcePack)
            {
                if (stardewItemName == "Money")
                {
                    return new LetterMoneyAttachment(receivedItem, resourcePackAmount);
                }

                var resourcePackItem = GetResourcePackItem(stardewItemName);
                if (resourcePackItem != null)
                {
                    return resourcePackItem.GetAsLetter(receivedItem, resourcePackAmount);
                }
            }

            if (TryParseFriendshipBonus(receivedItem.ItemName, out var numberOfPoints))
            {
                Game1.player.mailReceived.Add("ccBulletinThankYou");
                Game1.player.mailReceived.Add("ccBulletin%&NL&%");
                Game1.player.mailReceived.Add("ccBulletin");
                return new LetterActionAttachment(receivedItem, LetterActionsKeys.Friendship, numberOfPoints.ToString());
            }

            if (_trapManager.IsTrap(receivedItem.ItemName))
            {
                return _trapManager.GenerateTrapLetter(receivedItem);
            }

            if (receivedItem.ItemName.EndsWith(" Enzyme"))
            {
                return new LetterBuffAttachment(receivedItem, receivedItem.ItemName);
            }

            if (_unlockManager.IsUnlock(receivedItem.ItemName))
            {
                return _unlockManager.PerformUnlockAsLetter(receivedItem);
            }

            if (receivedItem.ItemName.EndsWith(RECIPE_SUFFIX))
            {
                var itemOfRecipe = receivedItem.ItemName[..^RECIPE_SUFFIX.Length];
                var recipe = _itemManager.GetRecipeByName(itemOfRecipe);
                if (recipe == null)
                {
                    _logger.LogError($"Could not process received recipe: {receivedItem.ItemName}. Generating an empty letter.");
                    return new LetterInformationAttachment(receivedItem);
                }
                return recipe.GetAsLetter(receivedItem);
            }

            var itemName = _nameMapper.GetInternalName(receivedItem.ItemName);
            if (_itemManager.ItemExists(itemName))
            {
                var singleItem = GetSingleItem(itemName);
                return singleItem.GetAsLetter(receivedItem);
            }

            return new LetterInformationAttachment(receivedItem);
            throw new ArgumentException($"Could not process item {receivedItem.ItemName}");
        }

        private bool TryParseResourcePack(string apItemName, out string stardewItemName, out int amount)
        {
            stardewItemName = "";
            amount = 0;
            if (apItemName.StartsWith(RESOURCE_PACK_PREFIX))
            {
                var apItemWithoutPrefix = apItemName.Substring(RESOURCE_PACK_PREFIX.Length);
                return TryParseResourcePack(apItemWithoutPrefix, out stardewItemName, out amount);
            }

            var parts = apItemName.Split(" ");
            if (!int.TryParse(parts[0], out amount))
            {
                return false;
            }

            var originalItemName = apItemName.Substring(apItemName.IndexOf(" ", StringComparison.Ordinal) + 1);
            stardewItemName = _nameMapper.GetInternalName(originalItemName);
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
            if (_itemManager.ItemExists(stardewItemName))
            {
                return _itemManager.GetItemByName(stardewItemName);
            }

            // Sometimes an item is plural because it's a resource pack, but the item is registered with a singular name in-game
            // So I try the alternate version before giving up
            var isPlural = stardewItemName.EndsWith('s');
            var otherVersion = isPlural ? stardewItemName.Substring(0, stardewItemName.Length - 1) : stardewItemName + "s";

            if (_itemManager.ItemExists(otherVersion))
            {
                return _itemManager.GetItemByName(otherVersion);
            }

            _logger.LogError($"Could not properly parse resource pack item: {stardewItemName}");
            return null;
        }
    }
}
