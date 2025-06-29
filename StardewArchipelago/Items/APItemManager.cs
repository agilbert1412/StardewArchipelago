using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items.Mail;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Items
{
    public class APItemManager : ItemManager
    {
        private readonly ItemParser _itemParser;
        private readonly Mailman _mail;

        public ItemParser ItemParser => _itemParser;
        public TrapManager TrapManager => _itemParser.TrapManager;

        public APItemManager(ILogger logger, IModHelper helper, Harmony harmony, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager, Mailman mail, TrapExecutor trapExecutor, GiftTrapManager giftTrapManager, IEnumerable<ReceivedItem> itemsAlreadyProcessed) : base(archipelago, itemsAlreadyProcessed)
        {
            _itemParser = new ItemParser(logger, helper, harmony, archipelago, locationChecker, itemManager, trapExecutor, giftTrapManager);
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

        public void MakeSureBackpacksAreFirst()
        {
            if (Game1.player.MaxItems > 6)
            {
                return;
            }

            var mailbox = Game1.player.mailbox.ToArray();
            mailbox = mailbox.OrderBy(x => x.Contains("Progressive_Backpack") ? 0 : 1).ToArray();
            Game1.player.mailbox.Clear();
            foreach (var mail in mailbox)
            {
                Game1.player.mailbox.Add(mail);
            }
        }
    }
}
