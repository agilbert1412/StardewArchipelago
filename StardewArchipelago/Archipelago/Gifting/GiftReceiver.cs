﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.Gifting.Net.Utilities.CloseTraitParser;
using Archipelago.Gifting.Net.Versioning.Gifts.Current;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Stardew;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Bundles;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;
using StardewValley;

namespace StardewArchipelago.Archipelago.Gifting
{
    public class GiftReceiver : IDisposable
    {
        private ILogger _logger;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;
        private readonly Mailman _mail;
        private readonly GiftProcessor _giftProcessor;

        public GiftReceiver(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager itemManager, Mailman mail, ICloseTraitParser<string> closeTraitParser, GiftTrapManager giftTrapManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _mail = mail;
            _giftProcessor = new GiftProcessor(logger, archipelago, itemManager, closeTraitParser, giftTrapManager);
            _archipelago.GiftingService.OnNewGift += ProcessNewGiftInstantly;
        }

        public void Dispose()
        {
            _archipelago.GiftingService.OnNewGift -= ProcessNewGiftInstantly;
        }

        public void ProcessNewGiftInstantly(Gift newGift)
        {
            if (_giftProcessor.ProcessGiftTrap(newGift))
            {
                _archipelago.GiftingService.RemoveGiftFromGiftBox(newGift.ID);
            }
        }

        public void ReceiveAllGifts()
        {
            var gifts = _archipelago.GiftingService.GetAllGiftsAndEmptyGiftBox();
            if (!gifts.Any())
            {
                return;
            }

            var giftAmounts = new Dictionary<ReceivedGift, int>();
            var giftIds = new Dictionary<string, ReceivedGift>();
            foreach (var (id, gift) in gifts)
            {
                ParseGift(gift, giftAmounts, giftIds);
            }

            foreach (var (receivedGift, amount) in giftAmounts)
            {
                var amountRemaining = amount;
                var amountInBundle = TrySendGiftToBundle(receivedGift, amountRemaining);
                if (amountInBundle > 0)
                {
                    amountRemaining -= amountInBundle;
                    if (amountRemaining <= 0)
                    {
                        continue;
                    }
                }
                while (amountRemaining > 0)
                {
                    amountRemaining = SendGiftMail(giftIds, receivedGift, amountRemaining);
                }
            }
        }

        /// <summary>
        ///     Sends a Gift Mail for the received gift item and the specified amount.
        /// </summary>
        /// <param name="giftIds"></param>
        /// <param name="receivedGift"></param>
        /// <param name="amount"></param>
        /// <returns>The amount of item remaining that needs to be sent after this gift, if the amount was too high</returns>
        private int SendGiftMail(Dictionary<string, ReceivedGift> giftIds, ReceivedGift receivedGift, int amount)
        {
            var relatedGiftIds = giftIds.Where(x => x.Value == receivedGift).Select(x => x.Key).ToArray();
            var senderGame = _archipelago.GetPlayerGame(receivedGift.SenderName);
            var item = _itemManager.GetItemByName(receivedGift.ItemName);
            var amountInGift = amount;
            if (amount > 999)
            {
                amountInGift = 999;
            }

            var amountRemainingAfterGift = amount - amountInGift;
            SendGiftMail(receivedGift.ItemName, receivedGift.SenderName, relatedGiftIds, amountRemainingAfterGift, item, amountInGift, senderGame);

            return amountRemainingAfterGift;
        }

        private void SendGiftMail(string itemName, string senderName, string[] relatedGiftIds, int amountRemainingAfterGift, StardewItem item, int amountInGift, string senderGame)
        {
            var mailKey = GetMailKey(relatedGiftIds, amountRemainingAfterGift);
            var embed = GetEmbed(item, amountInGift);
            SendGiftMail(itemName, senderName, senderGame, mailKey, embed);
        }

        public void ReceiveFakeGift(string objectId, int amount, string senderName)
        {
            var mailKey = GetMailKey(Guid.NewGuid().ToString(), amount);
            var embed = GetEmbed(objectId, amount);
            var stardewObject = _itemManager.GetObjectById(objectId);
            SendGiftMail(stardewObject.Name, senderName, "Stardew Valley", mailKey, embed);
        }

        public void SendGiftMail(string itemName, string senderName, string senderGame, string mailKey, string embed)
        {
            _mail.SendArchipelagoGiftMail(mailKey, itemName, senderName, senderGame, embed);
        }

        private void ParseGift(Gift gift, Dictionary<ReceivedGift, int> giftAmounts, Dictionary<string, ReceivedGift> giftIds)
        {
            if (_giftProcessor.ProcessGiftTrap(gift))
            {
                return;
            }

            if (!_giftProcessor.TryMakeStardewItem(gift, out var item, out var amount))
            {
                if (!gift.IsRefund)
                {
                    _archipelago.GiftingService.RefundGift(gift);
                }

                return;
            }

            var key = new ReceivedGift(item, gift.SenderSlot, _archipelago.GetPlayerName(gift.SenderSlot));
            if (!giftAmounts.ContainsKey(key))
            {
                giftAmounts.Add(key, 0);
            }

            giftAmounts[key] += amount;
            giftIds.Add(gift.ID, key);
        }

        private string GetEmbed(StardewItem item, int amount)
        {
            if (item == null || amount <= 0)
            {
                return "";
            }

            return GetEmbed(item.Id, amount);
        }

        private string GetEmbed(string objectId, int amount)
        {
            if (amount <= 0)
            {
                return "";
            }

            return $"%item object {objectId} {amount} %%";
        }

        private string GetMailKey(IEnumerable<string> ids, int amount)
        {
            return $"APGift;{string.Join(";", ids)};{amount}";
        }

        private string GetMailKey(string guid, int amount)
        {
            return $"APGift;{guid};{amount}";
        }

        public int TrySendGiftToBundle(ReceivedGift receivedGift, int amount)
        {
            if (!ArchipelagoJunimoNoteMenu.IsBundleRemaining(MemeBundleNames.COOPERATION))
            {
                return 0;
            }

            return ArchipelagoJunimoNoteMenu.TryDonateToBundle(MemeBundleNames.COOPERATION, receivedGift.ItemName, amount);
        }
    }
}
