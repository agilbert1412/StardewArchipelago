using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Items.Mail
{
    public class LetterVanillaAttachment : LetterAttachment
    {
        public string VanillaMailTitle { get; private set; }
        public bool NoLetter { get; private set; }

        protected override bool IsEmptyLetter => false;

        public LetterVanillaAttachment(ReceivedItem apItem, string mailTitle, bool noLetter) : base(apItem)
        {
            VanillaMailTitle = mailTitle;
            NoLetter = noLetter;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            mailman.SendVanillaMail(VanillaMailTitle, NoLetter);
        }
    }
}
