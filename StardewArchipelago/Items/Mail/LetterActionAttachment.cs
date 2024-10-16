﻿using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace StardewArchipelago.Items.Mail
{
    public class LetterActionAttachment : LetterAttachment
    {
        public string LetterOpenedAction { get; private set; }
        public string ActionParameter { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterActionAttachment(ReceivedItem apItem, string openedAction, string parameter = "") : base(apItem)
        {
            LetterOpenedAction = openedAction;
            ActionParameter = parameter;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            var key = GetMailKey();
            mailman.SendArchipelagoMail(key, ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString());
        }

        public override MailKey GetMailKey()
        {
            return new MailKey(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, LetterOpenedAction, ActionParameter, GetEmbedString(), ArchipelagoItem.UniqueId.ToString(), IsEmptyLetter);
        }
    }
}
