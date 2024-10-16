﻿using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace StardewArchipelago.Items.Mail
{
    public class LetterTrapAttachment : LetterAttachment
    {
        public string TrapName { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterTrapAttachment(ReceivedItem apItem, string trapName) : base(apItem)
        {
            TrapName = trapName;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            var key = GetMailKey();
            mailman.SendArchipelagoMail(key, ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }

        public override MailKey GetMailKey()
        {
            return new MailKey(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, LetterActionsKeys.Trap, TrapName, GetEmbedString(), ArchipelagoItem.UniqueId.ToString(), IsEmptyLetter);
        }
    }
}
