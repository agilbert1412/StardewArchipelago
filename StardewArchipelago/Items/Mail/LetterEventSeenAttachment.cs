using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.Items.Mail
{
    public class LetterEventSeenAttachment : LetterInformationAttachment
    {
        public List<int> EventToMarkAsSeen { get; private set; }

        protected override bool IsEmptyLetter => true;

        public LetterEventSeenAttachment(ReceivedItem apItem, int eventToMarkAsSeen) : base(apItem)
        {
            EventToMarkAsSeen = new List<int> {eventToMarkAsSeen};
        }

        public LetterEventSeenAttachment(ReceivedItem apItem, List<int> eventsToMarkAsSeen) : base(apItem)
        {
            EventToMarkAsSeen = eventsToMarkAsSeen;
        }

        public override void SendToPlayer(Mailman mailman)
        {
            foreach (var eventToSee in EventToMarkAsSeen)
            {
                Game1.player.eventsSeen.Add(eventToSee);
            }
            
            base.SendToPlayer(mailman);
        }
    }
}
