using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Gifts;
using Archipelago.Gifting.Net.Service;
using Archipelago.Gifting.Net.Traits;
using Microsoft.Xna.Framework;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftSender
    {
        private readonly IMonitor _monitor;
        private readonly ArchipelagoClient _archipelago;
        private readonly IGiftingService _giftService;
        internal GiftGenerator GiftGenerator { get; }

        public GiftSender(IMonitor monitor, ArchipelagoClient archipelago, StardewItemManager itemManager, IGiftingService giftService)
        {
            _monitor = monitor;
            _archipelago = archipelago;
            _giftService = giftService;
            GiftGenerator = new GiftGenerator(itemManager);
        }

        public void SendGift(string slotName, bool isTrap)
        {
            try
            {
                if (!_archipelago.PlayerExists(slotName))
                {
                    Game1.chatBox?.addMessage($"Could not find player named {slotName}", Color.Gold);
                    return;
                }

                var giftObject = Game1.player.ActiveObject;
                if (!GiftGenerator.TryCreateGiftItem(Game1.player.ActiveObject, isTrap, out var giftItem,
                        out var giftTraits))
                {
                    // TryCreateGiftItem will log the reason if it fails
                    return;
                }

                var isValidRecipient = _giftService.CanGiftToPlayer(slotName, giftTraits.Select(x => x.Trait));
                var giftOrTrap = isTrap ? "trap" : "gift";
                if (!isValidRecipient)
                {
                    Game1.chatBox?.addMessage($"{slotName} cannot receive this {giftOrTrap}", Color.Gold);
                    return;
                }

                var tax = GetTaxForItem(giftObject, out var itemValue);
                if (Game1.player.Money < tax)
                {
                    Game1.chatBox?.addMessage($"You cannot afford Joja Prime for this item", Color.Gold);
                    Game1.chatBox?.addMessage($"The tax is {_archipelago.SlotData.BankTax * 100}% of the item's value of {itemValue}g, so you must pay {tax}g to send it",
                        Color.Gold);
                    return;
                }


                var success = _giftService.SendGift(giftItem, giftTraits, slotName, out var giftId);
                _monitor.Log($"Sending {giftOrTrap} of {giftItem.Amount} {giftItem.Name} to {slotName} with {giftTraits.Length} traits. [ID: {giftId}]",
                    LogLevel.Info);
                if (!success)
                {
                    _monitor.Log($"Gift Failed to send properly", LogLevel.Error);
                    Game1.chatBox?.addMessage($"Unknown Error occurred while sending {giftOrTrap}.", Color.Red);
                    return;
                }

                Game1.player.ActiveObject = null;
                Game1.player.Money -= tax;
                Game1.chatBox?.addMessage(
                    $"{slotName} will receive your {giftOrTrap} of {giftItem.Amount} {giftItem.Name} within 1 business day",
                    Color.Gold);
                Game1.chatBox?.addMessage($"You have been charged a tax of {tax}g", Color.Gold);
                Game1.chatBox?.addMessage($"Thank you for using {GetDeliveryCompanyName()}", Color.Gold);
            }
            catch (Exception ex)
            {
                _monitor.Log($"Unknown error occurred while attempting to process gift command.{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", LogLevel.Error);
                Game1.chatBox?.addMessage($"Could not complete gifting operation. Check SMAPI for error details.", Color.Red);
                return;
            }
        }

        private int GetTaxForItem(Object giftObject)
        {
            return GetTaxForItem(giftObject, out _);
        }

        private int GetTaxForItem(Object giftObject, out int itemValue)
        {
            itemValue = giftObject.Price * giftObject.Stack;
            var tax = (int)Math.Round(_archipelago.SlotData.BankTax * itemValue);
            return tax;
        }

        public List<Object> SendShuffleGifts(Dictionary<string, List<Object>> targets)
        {
            var objectsFailedToSend = new List<Object>();
            var totalTax = 0;
            foreach (var (player, gifts) in targets)
            {
                foreach (var giftObject in gifts)
                {
                    TrySendShuffleGift(giftObject, player, objectsFailedToSend, out var tax);
                    totalTax += tax;
                }
            }

            Game1.player.Money = Math.Max(0, Game1.player.Money - totalTax);
            Game1.chatBox?.addMessage($"You have been charged a tax of {totalTax}g", Color.Gold);
            Game1.chatBox?.addMessage($"Thank you for using {GetDeliveryCompanyName()}", Color.Gold);

            return objectsFailedToSend;
        }

        private void TrySendShuffleGift(Object giftObject, string player, List<Object> objectsFailedToSend, out int tax)
        {
            tax = 0;
            try
            {
                if (!GiftGenerator.TryCreateGiftItem(giftObject, false, out var giftItem, out var giftTraits))
                {
                    objectsFailedToSend.Add(giftObject);
                    return;
                }

                giftTraits = giftTraits.Append(new GiftTrait("Shuffle", 1, 1)).ToArray();
                var success = _giftService.SendGift(giftItem, giftTraits, player, out var giftId);
                if (!success)
                {
                    objectsFailedToSend.Add(giftObject);
                    return;
                }

                tax = GetTaxForItem(giftObject);
            }
            catch (Exception ex)
            {
                objectsFailedToSend.Add(giftObject);
                _monitor.Log($"Unknown error occurred while attempting to gift.{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", LogLevel.Error);
                return;
            }
        }

        private string GetDeliveryCompanyName()
        {
            return "Joja Prime";
        }

        public List<string> GetAllPlayersThatCanReceiveGift(Object giftObject)
        {
            var validTargets = new List<string>();
            if (!GiftGenerator.TryCreateGiftItem(giftObject, false, out var giftItem, out var giftTraits))
            {
                return validTargets;
            }

            foreach (var player in _archipelago.Session.Players.AllPlayers)
            {
                if (player.Slot == _archipelago.CurrentPlayer.Slot)
                {
                    continue;
                }

                var isValidRecipient = _giftService.CanGiftToPlayer(player.Name, giftTraits.Select(x => x.Trait));
                if (isValidRecipient)
                {
                    validTargets.Add(player.Name);
                }
            }

            return validTargets;
        }
    }
}
