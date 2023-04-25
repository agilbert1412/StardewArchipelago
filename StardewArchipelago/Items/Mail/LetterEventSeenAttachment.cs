using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class LetterEventSeenAttachment : LetterInformationAttachment
    {
        public int EventToMarkAsSeen { get; private set; }

        public LetterEventSeenAttachment(ReceivedItem apItem, int eventToMarkAsSeen) : base(apItem)
        {
            EventToMarkAsSeen = eventToMarkAsSeen;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            Game1.player.eventsSeen.Add(EventToMarkAsSeen);
            base.SendToPlayer(mailman);
        }
    }
}
