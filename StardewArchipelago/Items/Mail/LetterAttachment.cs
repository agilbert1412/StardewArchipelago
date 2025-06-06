﻿using KaitoKid.ArchipelagoUtilities.Net.Client;

namespace StardewArchipelago.Items.Mail
{
    public abstract class LetterAttachment
    {
        public ReceivedItem ArchipelagoItem { get; private set; }
        protected abstract bool IsEmptyLetter { get; }

        public LetterAttachment(ReceivedItem apItem)
        {
            ArchipelagoItem = apItem;
        }

        public virtual string GetEmbedString()
        {
            return "";
        }

        public virtual void SendToPlayer(Mailman mailman)
        {
            mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, "");
        }

        public virtual MailKey GetMailKey()
        {
            return new MailKey(ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, GetEmbedString(),
                ArchipelagoItem.UniqueId.ToString(), IsEmptyLetter);
        }
    }
}
