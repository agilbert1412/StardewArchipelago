using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class LetterInformationAttachment : LetterAttachment
    {
        protected override bool IsEmptyLetter => true;

        public LetterInformationAttachment(ReceivedItem apItem) : base(apItem)
        {
        }
    }
}
