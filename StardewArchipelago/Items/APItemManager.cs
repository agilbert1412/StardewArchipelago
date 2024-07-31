using System.Collections.Generic;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Items
{
    public class APItemManager : ItemManager
    {
        private StardewArchipelagoClient _archipelago;
        private ItemParser _itemParser;
        private Mailman _mail;

        public ItemParser ItemParser => _itemParser;
        public TrapManager TrapManager => _itemParser.TrapManager;

        public APItemManager(ILogger logger, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager, Mailman mail, TileChooser tileChooser, BabyBirther babyBirther, GiftSender giftSender, IEnumerable<ReceivedItem> itemsAlreadyProcessed) : base(archipelago, itemsAlreadyProcessed)
        {
            _archipelago = archipelago;
            _itemParser = new ItemParser(logger, helper, harmony, archipelago, locationChecker, itemManager, tileChooser, babyBirther, giftSender);
            _mail = mail;
        }

        protected override void ProcessItem(ReceivedItem receivedItem, bool immediatelyIfPossible)
        {
            if (immediatelyIfPossible)
            {
                if (_itemParser.TrySendItemImmediately(receivedItem))
                {
                    return;
                }
            }

            var attachment = _itemParser.ProcessItemAsLetter(receivedItem);
            attachment.SendToPlayer(_mail);
        }
    }
}
