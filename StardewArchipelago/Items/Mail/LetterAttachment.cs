using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterAttachment
    {
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
            return $"AP|{ArchipelagoItem.ItemName}{ArchipelagoItem.PlayerName}{ArchipelagoItem.LocationName}";
        }
    }
}
