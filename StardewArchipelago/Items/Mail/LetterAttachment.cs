using System;
using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class LetterAttachment
    {
        protected static Random Random = new Random((int)Game1.uniqueIDForThisGame);
        public ReceivedItem ArchipelagoItem { get; private set; }

        public LetterAttachment(ReceivedItem apItem)
        {
            ArchipelagoItem = apItem;
        }

        public virtual string GetEmbedString()
        {
            return "";
        }

        public virtual void SendToPlayer(Mailman _mailman)
        {
            _mailman.SendArchipelagoMail(GetMailKey(), ArchipelagoItem.ItemName, ArchipelagoItem.PlayerName, ArchipelagoItem.LocationName, "");
        }

        public virtual string GetMailKey()
        {
            var key = $"AP|{ArchipelagoItem.ItemName}|{ArchipelagoItem.PlayerName}|{ArchipelagoItem.LocationName}|{Random.Next()}";
            var trimmedKey = key.Replace(" ", "_");
            return trimmedKey;
        }
    }
}
