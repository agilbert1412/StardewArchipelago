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
    }
}
